// Unity Ontology dAPI MonoBehaviour demo
// (c)2019 Joe Stewart

using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ONT_dAPI : MonoBehaviour
{
    public TextMesh welcomeText;
    public TextMesh providerText;
    public TextMesh ongBalanceText;
    public TextMesh ontBalanceText;
    public TextMesh supplyText;
    public TextMesh messageText;
    public TextMesh errorText;

    private string Address;
    private string OEP4Address = "APA4uEKJ8L7E5idi5oUfKbMYLc9NRBcByF"; // external address with OEP-4 balances
    private string PubKey;
    private string ONGBalance;
    private string ONTBalance;
    private string DICEHash = "9bb26ea4775e7e074d14360db3501654f6b028bc";
    private string Provider;
    private string Circulation;
    private string DICEName;
    private string TXID;
    private bool isReady = false;

    // dAPI call wrapper
    [DllImport("__Internal")]
    private static extern void dAPICall(string jparams);

    // message listener that passes data to callbacks
    [DllImport("__Internal")]
    private static extern void StartEventListener();

    private int _requestCount = 0;
    private Dictionary<string, string> ResultQueue;

    void Start()
    {
        ResultQueue = new Dictionary<string, string>();
        errorText.GetComponent<FadeText>().FadeIn("Waiting for dAPI-enabled wallet to be connected...");

#if UNITY_WEBGL && !UNITY_EDITOR
        StartEventListener();
        StartCoroutine(GetInfo());
#else
        //StartEventListener();
        //StartCoroutine(GetInfo());
        Debug.LogError("Must run in WebGL connecting to a running dAPI client");
#endif

    }

    public IEnumerator GetInfo()
    {
        yield return new WaitUntil(() => isReady);
        errorText.text = "";

      //  yield return SendRequest("getProvider", (result) => { Provider = JObject.Parse(result)["name"].ToString(); });
     //   providerText.GetComponent<FadeText>().FadeIn($"Connected to: {Provider}");

        //yield return SendRequest("getAccount", (result) => { Address = JObject.Parse(result)["address"].ToString(); });
        yield return SendRequest("getAccount", (result) => { Address = result; });
        welcomeText.GetComponent<FadeText>().FadeIn($"Welcome to Unity, {Address}");
        providerText.GetComponent<FadeText>().FadeIn($"Connected to: {Address}");

       
        // Get ONG and ONT balances
        yield return SendRequest("assets.getBalance", new { address = Address, 
            network = "MainNet" }, (result) => {
                //ONGBalance = JObject.Parse(result)["ONG"].ToString();
                ONGBalance = result;//JObject.Parse(result)["ONT"].ToString();
                Debug.Log($"ONGBalance result: {result}");
            });

        //ongBalanceText.GetComponent<FadeText>().FadeIn($"ONG Balance: {ONTBalance}");
        ontBalanceText.GetComponent<FadeText>().FadeIn($" Balance: {ONGBalance}");

        
        yield return SendRequest("assets.invokeRead", new
        {
            scriptHash= "9bb26ea4775e7e074d14360db3501654f6b028bc",
            operation=   "name",
            args = new List<object>() { new { type = "Integer", value = 5 }, new { type = "Integer", value = 4 }},
            gaslimit =  20000,
            gasprice=500
        }, (result) => {
            //ONGBalance = JObject.Parse(result)["ONG"].ToString();
            ONGBalance = result;//JObject.Parse(result)["ONT"].ToString();
        });
        ongBalanceText.GetComponent<FadeText>().FadeIn($"invoke: {ONGBalance}");
        

        /*
       // Get DICE balance of third party address
       yield return SendRequest("oep4.getBalanceOf", new {address = OEP4Address, scriptHash = DICEHash, network = "MainNet" },
           (result) => { ONTBalance = JObject.Parse(result)["amount"].ToString(); });

       // get in_circulation key from DICE contract storage
       yield return SendRequest("sc.getStorage", new { contract = DICEHash, key = "746f74616c537570706c79", network = "MainNet" },
           (res) => { Circulation = (HexToInt(res)/100000000).ToString(); });

       // get name of DICE token from contract
       yield return SendRequest("sc.invokeRead", new {
           scriptHash = DICEHash,
           operation = "name",
           arguments = new string[] { },
           network = "MainNet" },
           (result) => { DICEName = Encoding.ASCII.GetString(StringToByteArray(JObject.Parse(result)["Result"].ToString())); });
       supplyText.GetComponent<FadeText>().FadeIn($"Total {DICEName} supply: {Circulation}");

       yield return SendRequest("assets.send", new {
           to = Address,
           asset = "ONG",
           amount = "0.000000001",
           network = "MainNet"
       }, (result) => { TXID = JObject.Parse(result)["txid"].ToString(); });

       messageText.GetComponent<FadeText>().FadeIn($"Initiated ONG transfer in TX {TXID}");

       if (ONGBalance == "0")
       {
           errorText.GetComponent<FadeText>().FadeIn($"{Address} has zero ONG, transfer will fail");
       }
       */
    }

    public IEnumerator SendRequest(string request, Action<string> callback)
    {
        yield return SendRequest(request, "", callback);
    }

    public IEnumerator SendRequest(string request, object oparams, Action<string> callback)
    {
        string jparams = JsonConvert.SerializeObject(oparams);
        yield return SendRequest(request, jparams, callback);
    }

    public IEnumerator SendRequest(string request, string jparams, Action<string> callback)
    {
        _requestCount += 1;
        string reqid = _requestCount.ToString();
        Debug.Log($"Sending request {reqid}: {request}({jparams})");
        if (jparams == "")
        {
            dAPIRequest req = new dAPIRequest(request, reqid);
            dAPICall(JsonUtility.ToJson(req));
        }
        else
        {
            dAPIRequestWithParameters req = new dAPIRequestWithParameters(request, jparams, reqid);
            dAPICall(JsonUtility.ToJson(req));
        }
        Debug.Log($"WaitUntil {reqid}");
        yield return new WaitUntil(() => ResultQueue.ContainsKey(reqid));
        
        string result = ResultQueue[reqid];
        Debug.Log($"after WaitUntil {reqid},{result}");
        ResultQueue.Remove(reqid);
        callback(result);
    }

    public void dAPIResponseHandler(string jresponse)
    {
        dAPIResult response = JsonUtility.FromJson<dAPIResult>(jresponse);
        if (response.errorState)
        {
            Debug.LogError($"Request {response.requestId} failed: {response.resultData}");
            ResultQueue.Add(response.requestId, "");
            errorText.GetComponent<FadeText>().FadeIn(response.resultData);
        }
        else
        {
            Debug.Log($"Received {response.requestId} {response.resultData}");
            ResultQueue.Add(response.requestId, response.resultData);
        }
    }

    public void dAPIEventHandler(string jresponse)
    {
        dAPIEvent dapievent = JsonUtility.FromJson<dAPIEvent>(jresponse);
        if (dapievent.eventType == "READY")
        {
            isReady = true;
        }
        else if (dapievent.eventType == "DISCONNECTED")
        {
            errorText.text = "dAPI-enabled wallet disconnected!";
        }
        else if (dapievent.eventType == "ACCOUNT_CHANGED")
        {
            Address = JObject.Parse(dapievent.eventData)["address"].ToString();
            welcomeText.GetComponent<FadeText>().FadeIn($"Welcome to Unity, {Address}");
        }
        else
        {
            Debug.Log($"Unhandled event {dapievent.eventType}");
        }
    }

    private ulong HexToInt(string hex)
    {
       return BitConverter.ToUInt64(StringToByteArray(hex.PadRight(16, '0')), 0);
    }

    private byte[] StringToByteArray(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        int bl = bytes.Length;
        for (int i = 0; i < bl; ++i)
        {
            bytes[i] = (byte)((hex[2 * i] > 'F' ? hex[2 * i] - 0x57 : hex[2 * i] > '9' ? hex[2 * i] - 0x37 : hex[2 * i] - 0x30) << 4);
            bytes[i] |= (byte)(hex[2 * i + 1] > 'F' ? hex[2 * i + 1] - 0x57 : hex[2 * i + 1] > '9' ? hex[2 * i + 1] - 0x37 : hex[2 * i + 1] - 0x30);
        }
        return bytes;
    }

}

public class dAPIEvent
{
    public string eventType;
    public string eventData;

    public dAPIEvent(string _type, string _data)
    {
        eventType  = _type;
        eventData = _data;
    }
}

public class dAPIResult
{
    public string requestId;
    public string resultData;
    public bool errorState;

    public dAPIResult(string _id, string _data, bool _state)
    {
        requestId = _id;
        resultData = _data;
        errorState = _state;
    }
}

public class dAPIRequest
{
    public string name;
    public string reqid;

    public dAPIRequest(string _name, string _reqid)
    {
        name = _name;
        reqid = _reqid;
    }
}

public class dAPIRequestWithParameters
{
    public string name;
    public string config;
    public string reqid;

    public dAPIRequestWithParameters(string _name, string _config, string _reqid)
    {
        name = _name;
        config = _config;
        reqid = _reqid;
    }
}
