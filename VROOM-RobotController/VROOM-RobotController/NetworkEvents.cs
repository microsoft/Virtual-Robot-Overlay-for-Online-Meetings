using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace VROOM_RobotController
{
    [Serializable]
    public class NetworkEvent
    {
        public string EventName;
        public string EventData;
    }

    public class NetworkEvents
    {
        public bool AutoLogErrors = false;
        public string LocalPeerId;
        public string RemotePeerId;
        public string HttpServerAddress = "http://127.0.0.1:3000/";
        public int PollTimeMs = 50;

        private ConcurrentQueue<Action> _mainThreadWorkQueue = new ConcurrentQueue<Action>();

        public delegate void NetworkEventHandler(string data);

        private DateTime startTime;

        private Dictionary<string, NetworkEventHandler> networkEventHandlers = new Dictionary<string, NetworkEventHandler>();
        private Dictionary<string, float> timeSincePollMs = new Dictionary<string, float>();
        private Dictionary<string, bool> lastGetComplete = new Dictionary<string, bool>();

        public NetworkEvents(string newLocalPeerId,
            string newRemotePeerId,
            string newHttpServerAddress,
            bool newAutoLogErrors = false,
            int newPollTimeMs = 50)
        {
            this.LocalPeerId = newLocalPeerId;
            this.RemotePeerId = newRemotePeerId;
            this.HttpServerAddress = newHttpServerAddress;
            this.AutoLogErrors = newAutoLogErrors;
            this.PollTimeMs = newPollTimeMs;

            startTime = DateTime.Now;

            Thread updateLoopThread = new Thread(UpdateLoop);
            updateLoopThread.Start();
        }

        private void UpdateLoop()
        {
            while (true)
            {
                //DateTime lastTime = DateTime.Now;

                foreach (string eventName in networkEventHandlers.Keys)
                {
                    //DateTime currentTime = DateTime.Now;
                    //float deltaTime = (float)(TimeSpan.FromTicks(currentTime.Ticks).TotalMilliseconds - TimeSpan.FromTicks(lastTime.Ticks).TotalMilliseconds);

                    //// if we have not reached our PollTimeMs value...
                    //if (timeSincePollMs[eventName] <= PollTimeMs)
                    //{
                    //    // we keep incrementing our local counter until we do.
                    //    timeSincePollMs[eventName] += deltaTime;
                    //    continue;
                    //}

                    //// if we have a pending request still going, don't queue another yet.
                    //if (!lastGetComplete[eventName])
                    //{
                    //    continue;
                    //}

                    //// when we have reached our PollTimeMs value...
                    //timeSincePollMs[eventName] = 0f;

                    //// begin the poll and process.
                    //lastGetComplete[eventName] = false;

                    GetAndProcessFromServer(eventName);
                }

                Thread.Sleep(this.PollTimeMs);
            }
        }

        public bool AddHandler(string eventName, NetworkEventHandler eventHandler)
        {
            if (networkEventHandlers.ContainsKey(eventName))
            {
                return false;
            }
            else
            {
                networkEventHandlers.Add(eventName, eventHandler);
                return true;
            }
        }

        public bool RemoveHandler(string eventName)
        {
            if (networkEventHandlers.ContainsKey(eventName))
            {
                networkEventHandlers.Remove(eventName);
                return true;
            }
            else
            {
                return false;
            }
        }

        //public void PostEventMessage(NetworkEvent networkEvent)
        //{
        //    postEvtMssg(networkEvent);
        //}

        //private IEnumerator postEvtMssg(NetworkEvent networkEvent)
        //{
        //    Debug.Log(JsonUtility.ToJson(networkEvent));
        //    var data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(networkEvent));
        //    var www = new UnityWebRequest($"{HttpServerAddress}event/{RemotePeerId}", UnityWebRequest.kHttpVerbPOST);
        //    www.uploadHandler = new UploadHandlerRaw(data);

        //    yield return www.SendWebRequest();

        //    if (AutoLogErrors && (www.isNetworkError || www.isHttpError))
        //    {
        //        Debug.Log("Failure posting event: " + www.error);
        //    }
        //}

        private void GetAndProcessFromServer(string eventName)
        {
            try
            {
                WebRequest wrGETURL;
                wrGETURL = WebRequest.Create($"{HttpServerAddress}event/{LocalPeerId}/{eventName}");

                Stream objStream;
                objStream = wrGETURL.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);

                string respStr = "";
                string sLine = "";
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                        respStr += sLine;
                }

                if (respStr != "")
                {
                    NetworkEvent networkEvent = JsonConvert.DeserializeObject<NetworkEvent>(respStr);

                    // if the message is good
                    if (networkEvent != null)
                    {
                        NetworkEventHandler eventHandler;
                        if (networkEventHandlers.TryGetValue(networkEvent.EventName, out eventHandler))
                        {
                            eventHandler.Invoke(networkEvent.EventData);
                        }
                    }
                    else if (AutoLogErrors)
                    {
                        Console.WriteLine($"Failed to deserialize JSON message : {respStr}");
                    }
                }
                else
                {
                    // This is very spammy because the node-dss protocol uses 404 as regular "no data yet" message, which is an HTTP error
                    //Debug.LogError($"HTTP error: {www.error}");
                }
            }
            catch (Exception e)
            {
                ///
            }
            finally
            {
                lastGetComplete[eventName] = true;
            }
        }
    }
}
