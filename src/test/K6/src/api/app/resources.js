import http from "k6/http";
import * as config from "../../config.js";
import * as header from "../../buildrequestheaders.js"


//Batch Api calls after instance creation to get app resources like Appmetadata, Formlayoust.json, rulehandler.js, ruleconfiguration.json
export function batchGetAppResources(altinnStudioRuntimeCookie){
    let req, res;
    var requestParams = header.buildHearderWithRuntime(altinnStudioRuntimeCookie, "app")
    req = [
        {
            "method": "get",
            "url": config.appResources["servicemetadata"],
            "params": requestParams        
        },{
            "method": "get",
            "url": config.appResources["formlayout"],
            "params": requestParams
        },{
            "method": "get",
            "url": config.appResources["rulehandler"],
            "params": requestParams        
        },{
            "method": "get",
            "url": config.appResources["ruleconfiguration"],
            "params": requestParams
        }];
    res = http.batch(req);
    return res;
}