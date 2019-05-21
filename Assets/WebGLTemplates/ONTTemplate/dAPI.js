// Unity dAPI client library
// (c)2019 Joe Stewart

let dapi;
var client = dApi.client;
function initdAPI()
{
  client.registerClient({});
  postEvent("READY", "");
  // o3dapi.initPlugins([o3dapiOnt]);
  // dapi = window.o3dapi.ONT;

  // o3dapi.ONT.addEventListener(o3dapi.ONT.Constants.EventName.READY, 
  //   data => {
  //     console.log(`dAPI provider ready: ${data.name}`);
  //     postEvent("READY", JSON.stringify(data));
  // });

  // o3dapi.ONT.addEventListener(o3dapi.ONT.Constants.EventName.ACCOUNT_CHANGED, 
  //   data => {
  //     console.log(`Changed Account: ${data.address}`);
  //     postEvent("ACCOUNT_CHANGED", JSON.stringify(data));
  // });

  // o3dapi.ONT.addEventListener(o3dapi.ONT.Constants.EventName.CONNECTED, 
  //   data => {
  //     console.log(`Connected Account: ${data.address}`);
  //     postEvent("CONNECTED", JSON.stringify(data));
  // });

  // o3dapi.ONT.addEventListener(o3dapi.ONT.Constants.EventName.DISCONNECTED, 
  //   data => {
  //     console.log(`dAPI provider disconnected`);
  //     postEvent("DISCONNECTED", "{}");
  // });

  // o3dapi.ONT.addEventListener(o3dapi.ONT.Constants.EventName.NETWORK_CHANGED, 
  //   data => {
  //     console.log(`Network changed: ${data.defaultNetwork}`);
  //     postEvent("NETWORK_CHANGED", JSON.stringify(data));
  // });
}

function BrowserdAPICall(jparams) {
  var api = JSON.parse(jparams);
  if(api["name"].indexOf("getAccount")>-1){
    client.api.asset.getAccount().then(function(result){
      postResult(api.reqid, result, false) 
    })
  } else if(api["name"].indexOf("getBalance")>-1) {
    client.api.network.getBalance({ address: JSON.parse(api["config"])["address"] }).then(function(result){
      postResult(api.reqid, JSON.stringify(result), false) 
    }).catch(err => {
      console.log(err)
    })
  } else if(api["name"].indexOf("invokeRead")>-1) {
    var config = JSON.parse(api["config"]);
    const scriptHash = config["scriptHash"];
    const operation = config["operation"];
    const args = config["args"];//[{ type: 'Integer', value: 5 }, { type: 'Integer', value: 4 }];
    const gasPrice = 500;
    const gasLimit = 30000;
    client.api.smartContract.invokeRead({ scriptHash, operation, args, gasPrice, gasLimit }).then(function(result){
      postResult(api.reqid, JSON.stringify(result), false) 
    }).catch(err => {
      console.log(err)
    })
  } else if(api["name"].indexOf("invoke")>-1) {
    var config = JSON.parse(api["config"]);
    const scriptHash = 'fe7a542bd4f1ae71d42c4b15480fb2f421c7631b';
    const operation = 'Add';
    const args = [{ type: 'Integer', value: 5 }, { type: 'Integer', value: 4 }];
    const gasPrice = 500;
    const gasLimit = 30000;
    client.api.smartContract.invoke({ scriptHash, operation, args, gasPrice, gasLimit }).then(function(result){
      postResult(api.reqid, JSON.stringify(result), false) 
    }).catch(err => {
      console.log(err)
    })
  }  else {
    postResult(api.reqid, "error of c# request", false)
  }

  // var api = JSON.parse(jparams);
  // if (("name" in api) && ("config" in api) && ("reqid" in api))
  // { 
  //   // api call with parameters
  //   let dapiCall;
  //   if (api.name.includes("."))
  //   {
  //       var lib = api.name.split(".")[0];
  //       var subcall = api.name.split(".")[1];
  //       dapiCall = dapi[lib][subcall];
  //   } else
  //   {
  //       dapiCall = dapi[api.name];
  //   }
    
  //   dapiCall(JSON.parse(api.config))
  //   .then((result) => { 
  //         result = typeof result !== "string"
  //       ? JSON.stringify(result)
  //       : result;
  //      postResult(api.reqid, result, false) })
  //   .catch((err) => { 
  //     var error = typeof err !== "string"
  //       ? `${err.type}: ${err.message}`
  //       : `dAPI call failed: ${err}`;
  //     postResult(api.reqid, error, true);
  //  });
  // } else if (("name" in api) && ("reqid" in api))
  // { 
  //   // api call without parameters
  //   dapi[api.name]()
  //   .then((result) => { 
  //         result = typeof result !== "string"
  //       ? JSON.stringify(result)
  //       : result;
  //      postResult(api.reqid, result, false) })
  //   .catch((err) => { 
  //     var error = typeof err !== "string"
  //       ? `${err.type}: ${err.message}`
  //       : `dAPI call failed: ${err}`;
  //     postResult(api.reqid, error, true) 
  //   });
  // }
  // else if ("reqid" in api)
  // {
  //   postResult(api.reqid, 'Invalid API name', true);
  // }
  // else 
  // {
  //   postResult('-1', 'Missing request ID', true);
  // }
}

function postResult(id, res, state) {
  if ((state) && (res == ""))
  {
    res = "Unknown error";
  }
  var msg = {
    requestId: id,
    resultData: res,
    errorState: state 
  };
  window.postMessage(msg, "*");
}

function postEvent(type, data) {
  var msg = {
    eventType: type,
    eventData: data
  };
  window.postMessage(msg, "*");
}  

