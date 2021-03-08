/**
 *
 * main.js
 * Main JavaScript code
 *
 */


'use strict';

jQuery(function()
{
  var url = window.location.hostname;
  var mainServerUrl = "https://" + url + ":8082";
  var dssServerUrl = "http://" + url + ":3000";
  
  var doc = jQuery(document),
      win = jQuery(window);

  // const signaling = new SignalingChannel();
  const constraints =
  {
    audio: true,
    //video: true
    //video: { width: 3840, height: 1920 }
    video: { width: 1920, height: 960 }
  };
  const configuration =
  {
    // iceServers: [
    // {
    //   url: 'turn:numb.viagenie.ca',
    //   credential: 'dvc',
    //   username: 'brennandgj@gmail.com',
    //   password: 'dvcchat'
    // }]
  };
  const pc = new RTCPeerConnection(configuration);

  const POLL_SERVER_TIMEOUT = 200;

  var iceCandidateQueue = [];


  /**
   * CALL
   */

  // Send any ice candidates to the other peer.
  // pc.onicecandidate = ({candidate}) => signaling.send({candidate});
  pc.onicecandidate = function (event)
  {
    if (event.candidate)
    {
      PostToServer(
      {
        'MessageType': 3, // ICE
        'Data': event.candidate + "|" + event.candidate.sdpMLineIndex.toString(10) + "|" + event.candidate.sdpMid,
        'IceDataSeparator': "|"
      });
    }
  };

  // Let the "negotiationneeded" event trigger offer generation.
  pc.onnegotiationneeded = async () =>
  {
    // try
    // {
    //   await pc.setLocalDescription(await pc.createOffer());
    //   // send the offer to the other peer
    //   // signaling.send({desc: pc.localDescription});
    //   PostToServer(
    //   {
    //     'MessageType': 1,  // offer
    //     'Data': pc.localDescription.sdp
    //   });
    // }
    // catch (err)
    // {
    //   console.error(err);
    // }
  };

  function setOnTrackOrStreamHandler(peer, handler)
  {
    if ('ontrack' in peer)
    {
      peer.ontrack = function(event)
      {
        console.log('ontrack');

        var remoteStream = event.streams[0];
        handler(remoteStream);
      }
    }
    else
    {
      peer.onaddstream = function(event)
      {
        console.log('onaddstream');

        var remoteStream = event.stream;
        handler(remoteStream);
      }
    }
  }

  function playRemoteStream(stream)
  {
    $('#remoteVideo').prop('srcObject', stream);
  }

  
  function SetupLocalStream()
  {
    // Get audio/video stream
    navigator.mediaDevices.getUserMedia(constraints).then(function(stream)
    {
      stream.getTracks().forEach((track) => pc.addTrack(track, stream));
      window.localStream = stream;

      $('#localVideo').prop('srcObject', window.localStream);

      setOnTrackOrStreamHandler(pc, (remoteStream) =>
      {
        playRemoteStream(remoteStream);
      });

      PollServer();
    })
    .catch(function(err)
    {
      console.log("Error setting up local stream: " + err);
    });
  }

  function PostToServer(msg)
  {
    $.ajax(
    {
      url: dssServerUrl + "/data/viewer",
      type: 'POST',
      //dataType: 'json',
      //contentType: 'application/json; charset=utf-8',
      data: JSON.stringify(msg),
      cache: false,
      processData: false
    })
    .done(function(response)
    {
      console.log("POST Response: " + response);
    });
  }

  async function GetAndProcessFromServer()
  {
    $.ajax(
    {
      type: 'GET',
      //dataType: 'json',
      url: dssServerUrl + "/data/broadcaster",
      success: function(msgStr)
      {
        var msg = JSON.parse(msgStr);

        switch (msg.MessageType)
        {
          case 1:   // offer
              pc.setRemoteDescription(new RTCSessionDescription(
              {
                'type': 'offer',
                'sdp': msg.Data
              }))
              .then(function()
              {
                pc.createAnswer(function(result)
                {
                  pc.setLocalDescription(result, function()
                  {
                    while (iceCandidateQueue.length > 0)
                    {
                      pc.addIceCandidate(iceCandidateQueue.shift());
                    }

                    //signaling.send({desc: pc.localDescription});
                    PostToServer(
                    {
                      'MessageType': 2,
                      'Data': pc.localDescription.sdp
                    });
                  },
                  function()
                  {
                    console.log("setLocalDescription error");
                  });
                },
                function(error)
                {
                  console.log("createAnswer error");
                });
              });
              
              break;
          case 2:   // answer
              pc.setRemoteDescription(new RTCSessionDescription(
              {
                'type': 'answer',
                'sdp': msg.Data
              }))
              .then(function()
              {
                ///
              });
              
              break;
          case 3:   // ICE
              var parts = msg.Data.split(msg.IceDataSeparator);
              
              var candidateObj = new RTCIceCandidate(
              {
                candidate: parts[0],
                sdpMLineIndex: parts[1],
                sdpMid: parts[2]
              });
              
              if(!pc || !pc.remoteDescription || !pc.remoteDescription.type)
              {
                iceCandidateQueue.push(candidateObj);
              }
              else
              {
                pc.addIceCandidate(candidateObj);
              }
              
              break;
          default:
              console.log("Unknown message: " + msg.MessageType + ": " + msg.Data);

              break;
        }
      },
      error: function (xhr, ajaxOptions, thrownError)
      {
          // if(xhr.status==404)
          // {
          //     console.log(thrownError);
          // }
      }
    });
  }

  function PollServer()
  {
    GetAndProcessFromServer();
    setTimeout(PollServer, POLL_SERVER_TIMEOUT);
  }


  /**
   * MAIN
   */

  SetupLocalStream();
});
