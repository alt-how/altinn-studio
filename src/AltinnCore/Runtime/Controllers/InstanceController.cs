using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altinn.Platform.Storage.Models;
using AltinnCore.Common.Attributes;
using AltinnCore.Common.Configuration;
using AltinnCore.Common.Helpers;
using AltinnCore.Common.Models;
using AltinnCore.Common.Services.Interfaces;
using AltinnCore.ServiceLibrary.Api;
using AltinnCore.ServiceLibrary.Enums;
using AltinnCore.ServiceLibrary.Models;
using AltinnCore.ServiceLibrary.Models.Workflow;
using AltinnCore.ServiceLibrary.Services.Interfaces;
using Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Storage.Interface.Enums;

namespace AltinnCore.Runtime.Controllers
{
    /// <summary>
    /// This is the controller responsible for handling all runtime service events
    /// </summary>
    public class InstanceController : Controller
    {
        private readonly IRepository _repository;
        private readonly IApplication _application;
        private readonly IAuthorization _authorization;
        private readonly IRegister _register;
        private readonly IProfile _profile;
        private readonly IER _er;
        private readonly ILogger _logger;
        private readonly IForm _form;
        private readonly IExecution _execution;
        private readonly IArchive _archive;
        private readonly ITestdata _testdata;
        private readonly UserHelper _userHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkflow _workflowSI;
        private readonly IInstance _instance;
        private readonly IPlatformServices _platformSI;
        private readonly IData _data;
        private readonly IPrefill _prefill;
        private readonly ServiceRepositorySettings _settings;
        private readonly GeneralSettings _generalSettings;

        private const string FORM_ID = "default";
        private const long REQUEST_SIZE_LIMIT = 2000 * 1024 * 1024;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceController"/> class
        /// </summary>
        /// <param name="authorizationService">The authorizationService (set in Startup.cs)</param>
        /// <param name="logger">The logger (set in Startup.cs)</param>
        /// <param name="profileService">The profile service (set in Startup.cs)</param>
        /// <param name="registerService">The registerService (set in Startup.cs)</param>
        /// <param name="erService">The erService (set in Startup.cs)</param>
        /// <param name="formService">The form</param>
        /// <param name="repositoryService">The repository service (set in Startup.cs)</param>
        /// <param name="serviceExecutionService">The serviceExecutionService (set in Startup.cs)</param>
        /// <param name="archiveService">The archive service</param>
        /// <param name="httpContextAccessor">The http context accessor</param>
        /// <param name="testDataService">the test data service handler</param>
        /// <param name="workflowSI">the workflow service handler</param>
        /// <param name="instanceSI">the instance service handler</param>
        /// <param name="platformSI">the platform service handler</param>
        /// <param name="dataSI">the data service handler</param>
        /// <param name="application">the application service handler</param>
        /// <param name="prefill">The prefill service handler</param>
        /// <param name="repositorySettings">the repository settings</param>
        /// <param name="generalSettings">the general settings</param>
        public InstanceController(
            IAuthorization authorizationService,
            ILogger<InstanceController> logger,
            IProfile profileService,
            IRegister registerService,
            IER erService,
            IForm formService,
            IRepository repositoryService,
            IExecution serviceExecutionService,
            IArchive archiveService,
            ITestdata testDataService,
            IHttpContextAccessor httpContextAccessor,
            IWorkflow workflowSI,
            IInstance instanceSI,
            IPlatformServices platformSI,
            IData dataSI,
            IApplication application,
            IPrefill prefill,
            IOptions<ServiceRepositorySettings> repositorySettings,
            IOptions<GeneralSettings> generalSettings)
        {
            _authorization = authorizationService;
            _logger = logger;
            _profile = profileService;
            _register = registerService;
            _er = erService;
            _form = formService;
            _repository = repositoryService;
            _execution = serviceExecutionService;
            _userHelper = new UserHelper(profileService, _register, generalSettings);
            _archive = archiveService;
            _testdata = testDataService;
            _httpContextAccessor = httpContextAccessor;
            _workflowSI = workflowSI;
            _instance = instanceSI;
            _platformSI = platformSI;
            _data = dataSI;
            _prefill = prefill;
            _application = application;
            _settings = repositorySettings.Value;
            _generalSettings = generalSettings.Value;
        }

        /// <summary>
        /// Action used for SPA
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="instanceGuid">the instance guid</param>
        /// <param name="view">name of the view</param>
        /// <param name="itemId">the item id</param>
        /// <returns>The react view or the receipt</returns>
        [Authorize]
        public async Task<IActionResult> EditSPA(string org, string app, Guid instanceGuid, string view, int? itemId)
        {
            ViewBag.Org = org;
            ViewBag.App = app;

            // TODO Add info for REACT app.
            return View();
        }

        /// <summary>
        /// This is the HttpPost version of the CompleteAndSendIn operation that
        /// is triggered when user press the send in option.
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="partyId">The partyId.</param>
        /// <param name="instanceGuid">The instanceGuid.</param>
        /// <param name="view">The ViewName.</param>
        /// <returns>Redirect user to the receipt page.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CompleteAndSendIn(string org, string app, int partyId, Guid instanceGuid, string view)
        {
            // Dependency Injection: Getting the Service Specific Implementation based on the app parameter data store
            // Will compile code and load DLL in to memory for AltinnCore
            IServiceImplementation serviceImplementation = _execution.GetServiceImplementation(org, app, false);

            // Get the serviceContext containing all metadata about current app.
            ServiceContext serviceContext = _execution.GetServiceContext(org, app, false);

            // Create and populate the RequestContext object and make it available for the service implementation so
            // app developer can implement logic based on information about the request and the user performing
            // the request
            RequestContext requestContext = await PopulateRequestContext(instanceGuid);

            serviceImplementation.SetPlatformServices(_platformSI);

            // Assign data to the ViewBag so it is available to the service views or service implementation
            PopulateViewBag(org, app, instanceGuid, 0, requestContext, serviceContext, _platformSI);

            // Getting the Form Data
            Instance instance = await _instance.GetInstance(app, org, requestContext.UserContext.PartyId, instanceGuid);
            Guid.TryParse(instance.Data.Find(m => m.ElementType == FORM_ID).Id, out Guid dataId);
            object serviceModel = _data.GetFormData(instanceGuid, serviceImplementation.GetServiceModelType(), org, app, requestContext.UserContext.PartyId, dataId);
            serviceImplementation.SetServiceModel(serviceModel);

            ViewBag.FormID = instanceGuid;
            ViewBag.ServiceContext = serviceContext;

            serviceImplementation.SetContext(requestContext, serviceContext, null, ModelState);
            await serviceImplementation.RunServiceEvent(ServiceEventType.Validation);

            ApiResult apiResult = new ApiResult();
            if (ModelState.IsValid)
            {
                ServiceState currentState = _workflowSI.MoveServiceForwardInWorkflow(instanceGuid, org, app, requestContext.UserContext.PartyId);

                if (currentState.State == WorkflowStep.Archived)
                {
                    Instance archivedInstance = await _instance.ArchiveInstance(serviceModel, serviceImplementation.GetServiceModelType(), app, org, requestContext.UserContext.PartyId, instanceGuid);
                    apiResult.NextState = currentState.State;
                    apiResult.Instance = archivedInstance;
                }
            }

            ModelHelper.MapModelStateToApiResult(ModelState, apiResult, serviceContext);

            if (apiResult.Status.Equals(ApiStatusType.ContainsError))
            {
                Response.StatusCode = 202;
            }
            else
            {
                Response.StatusCode = 200;
            }

            return new ObjectResult(apiResult);
        }

        /// <summary>
        /// Action method to present.
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="partyId">The partyId.</param>
        /// <param name="instanceGuid">The instanceGuid.</param>
        /// <returns>The receipt view.</returns>
        public async Task<IActionResult> Receipt(string org, string app, int partyId, Guid instanceGuid)
        {
            // Dependency Injection: Getting the Service Specific Implementation based on the app parameter data store
            // Will compile code and load DLL in to memory for AltinnCore
            IServiceImplementation serviceImplementation = _execution.GetServiceImplementation(org, app, false);

            // Get the serviceContext containing all metadata about current app.
            ServiceContext serviceContext = _execution.GetServiceContext(org, app, false);

            // Create and populate the RequestContext object and make it available for the service implementation so
            // app developer can implement logic based on information about the request and the user performing
            // the request
            RequestContext requestContext = RequestHelper.GetRequestContext(Request.Query, instanceGuid);
            requestContext.UserContext = await _userHelper.GetUserContext(HttpContext);
            requestContext.Party = requestContext.UserContext.Party;

            serviceImplementation.SetPlatformServices(_platformSI);

            // Assign data to the ViewBag so it is available to the app views or service implementation
            PopulateViewBag(org, app, instanceGuid, 0, requestContext, serviceContext, _platformSI);

            object serviceModel = _archive.GetArchivedServiceModel(instanceGuid, serviceImplementation.GetServiceModelType(), org, app, requestContext.Party.PartyId);
            List<ServiceInstance> formInstances = _testdata.GetFormInstances(requestContext.Party.PartyId, org, app);
            string properInstanceId = $"{requestContext.Party}/{instanceGuid}";

            ViewBag.ServiceInstance = formInstances.Find(i => i.ServiceInstanceID == properInstanceId);

            return View();
        }

        /// <summary>
        /// The start Service operation used to start apps.
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <returns>The start service View.</returns>
        [Authorize]
        public async Task<IActionResult> StartService(string org, string app)
        {
            UserContext userContext = await _userHelper.GetUserContext(HttpContext);
            var startServiceModel = new StartServiceModel
            {
                PartyList = _authorization
                    .GetPartyList(userContext.UserId)
                    .Select(x => new SelectListItem
                    {
                        Text = (x.PartyTypeName == PartyType.Person) ? x.SSN + " " + x.Name : x.OrgNumber + " " + x.Name,
                        Value = x.PartyId.ToString(),
                    })
                    .ToList(),
                ServiceID = org + "_" + app,
            };
            return View(startServiceModel);
        }

        /// <summary>
        /// This is the post operation to instantiate an instance
        /// </summary>
        /// <param name="startServiceModel">The start service model.</param>
        /// <returns>JSON Object with the instance ID</returns>
        [Authorize]
        [HttpPost]
        public async Task<dynamic> InstantiateApp(StartServiceModel startServiceModel)
        {
            // Dependency Injection: Getting the Service Specific Implementation based on the app parameter data store
            // Will compile code and load DLL in to memory for AltinnCore
            bool startService = true;
            IServiceImplementation serviceImplementation = _execution.GetServiceImplementation(startServiceModel.Org, startServiceModel.Service, startService);

            // Get the service context containing metadata about the app
            ServiceContext serviceContext = _execution.GetServiceContext(startServiceModel.Org, startServiceModel.Service, startService);

            // Create and populate the RequestContext object and make it available for the service implementation so
            // app developer can implement logic based on information about the request and the user performing
            // the request
            RequestContext requestContext = RequestHelper.GetRequestContext(Request.Query, Guid.Empty);
            requestContext.UserContext = await _userHelper.GetUserContext(HttpContext);

            // Populate the reportee information
            requestContext.UserContext.Party = await _register.GetParty(startServiceModel.PartyId);
            requestContext.Party = requestContext.UserContext.Party;

            // Checks if the reportee is allowed to instantiate the application
            Application application = await _application.GetApplication(startServiceModel.Org, startServiceModel.Service);
            if (application == null || (application != null && !InstantiationHelper.IsPartyAllowedToInstantiate(requestContext.UserContext.Party, application.PartyTypesAllowed)))
            {
                 return new StatusCodeResult(403);
            }

            // Create platform service and assign to service implementation making it possible for the service implementation
            // to use platform services. Also make it available in ViewBag so it can be used from Views
            serviceImplementation.SetPlatformServices(_platformSI);

            // Assign the different context information to the service implementation making it possible for
            // the app developer to take use of this information
            serviceImplementation.SetContext(requestContext, serviceContext, null, ModelState);

            object serviceModel = null;

            if (!string.IsNullOrEmpty(startServiceModel.PrefillKey))
            {
                _form.GetPrefill(
                    startServiceModel.Org,
                    startServiceModel.Service,
                    serviceImplementation.GetServiceModelType(),
                    startServiceModel.PartyId,
                    startServiceModel.PrefillKey);
            }

            if (serviceModel == null)
            {
                // If the service model was not loaded from prefill.
                serviceModel = serviceImplementation.CreateNewServiceModel();
            }

            // Assign service model to the implementation
            serviceImplementation.SetServiceModel(serviceModel);

            // Run prefill
            PrefillContext prefillContext = new PrefillContext
            {
                Organization = requestContext.UserContext.Party.Organization,
                Person = requestContext.UserContext.Party.Person,
                UserId = requestContext.UserContext.UserId,
                Org = startServiceModel.Org,
                App = startServiceModel.Service,
            };
            await _prefill.PrefillDataModel(prefillContext, serviceModel);

            // Run Instansiation event
            await serviceImplementation.RunServiceEvent(ServiceEventType.Instantiation);

            // Run validate Instansiation event where
            await serviceImplementation.RunServiceEvent(ServiceEventType.ValidateInstantiation);

            // If ValidateInstansiation event has not added any errors the new form is saved and user is redirercted to the correct
            if (ModelState.IsValid)
            {
                if (serviceContext.WorkFlow.Any() && serviceContext.WorkFlow[0].StepType.Equals(StepType.Lookup))
                {
                    return JsonConvert.SerializeObject(
                        new
                        {
                            org = startServiceModel.Org,
                            service = startServiceModel.Service
                        });
                }

                int instanceOwnerId = requestContext.UserContext.PartyId;

                // Create a new instance document
                Instance instance = await _instance.InstantiateInstance(startServiceModel, serviceModel, serviceImplementation);

                // Create and store the instance created event
                InstanceEvent instanceEvent = new InstanceEvent
                {
                    AuthenticationLevel = requestContext.UserContext.AuthenticationLevel,
                    EventType = InstanceEventType.Created.ToString(),
                    InstanceId = instance.Id,
                    InstanceOwnerId = instanceOwnerId.ToString(),
                    UserId = requestContext.UserContext.UserId,
                    ProcessInfo = instance.Process,
                };

                Enum.TryParse<WorkflowStep>(instance.Process.CurrentTask.Name, out WorkflowStep currentStep);

                return JsonConvert.SerializeObject(
                    new
                    {
                        instanceId = instance.Id,
                    });
            }

            startServiceModel.PartyList = _authorization.GetPartyList(requestContext.UserContext.UserId)
               .Select(x => new SelectListItem
               {
                   Text = (x.PartyTypeName == PartyType.Person) ? x.SSN + " " + x.Name : x.OrgNumber + " " + x.Name,
                   Value = x.PartyId.ToString(),
               }).ToList();

            HttpContext.Response.Cookies.Append(_generalSettings.GetAltinnPartyCookieName, startServiceModel.PartyId.ToString());

            return JsonConvert.SerializeObject(
                new
                {
                    redirectUrl = View(startServiceModel),
                });
        }

        /// <summary>
        /// This is the post operation for the start service.
        /// </summary>
        /// <param name="startServiceModel">The start service model.</param>
        /// <returns>Redirects to the new instance of a service or lookup service view.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StartService(StartServiceModel startServiceModel)
        {
            // Dependency Injection: Getting the Service Specific Implementation based on the service parameter data store
            // Will compile code and load DLL in to memory for AltinnCore
            bool startService = true;
            IServiceImplementation serviceImplementation = _execution.GetServiceImplementation(startServiceModel.Org, startServiceModel.Service, startService);

            // Get the service context containing metadata about the app
            ServiceContext serviceContext = _execution.GetServiceContext(startServiceModel.Org, startServiceModel.Service, startService);

            // Create and populate the RequestContext object and make it available for the service implementation so
            // app developer can implement logic based on information about the request and the user performing
            // the request
            RequestContext requestContext = RequestHelper.GetRequestContext(Request.Query, Guid.Empty);
            requestContext.UserContext = await _userHelper.GetUserContext(HttpContext);

            // Populate the reportee information
            requestContext.UserContext.Party = await _register.GetParty(startServiceModel.PartyId);
            requestContext.Party = requestContext.UserContext.Party;

            // Create platform service and assign to service implementation making it possible for the service implementation
            // to use platform services. Also make it available in ViewBag so it can be used from Views
            serviceImplementation.SetPlatformServices(_platformSI);

            // Assign the different context information to the service implementation making it possible for
            // the app developer to take use of this information
            serviceImplementation.SetContext(requestContext, serviceContext, null, ModelState);

            object serviceModel = null;

            if (!string.IsNullOrEmpty(startServiceModel.PrefillKey))
            {
                _form.GetPrefill(
                    startServiceModel.Org,
                    startServiceModel.Service,
                    serviceImplementation.GetServiceModelType(),
                    startServiceModel.PartyId,
                    startServiceModel.PrefillKey);
            }

            if (serviceModel == null)
            {
                // If the service model was not loaded from prefill.
                serviceModel = serviceImplementation.CreateNewServiceModel();
            }

            // Assign service model to the implementation
            serviceImplementation.SetServiceModel(serviceModel);

            // Run Instansiation event
            await serviceImplementation.RunServiceEvent(ServiceEventType.Instantiation);

            // Run validate Instansiation event where
            await serviceImplementation.RunServiceEvent(ServiceEventType.ValidateInstantiation);

            // If ValidateInstansiation event has not added any errors the new form is saved and user is redirercted to the correct
            if (ModelState.IsValid)
            {
                if (serviceContext.WorkFlow.Any() && serviceContext.WorkFlow[0].StepType.Equals(StepType.Lookup))
                {
                    return RedirectToAction("Lookup", new { org = startServiceModel.Org, app = startServiceModel.Service });
                }

                int instanceOwnerId = requestContext.UserContext.PartyId;

                // Create a new instance document
                Instance instance = await _instance.InstantiateInstance(startServiceModel, serviceModel, serviceImplementation);

                // Create and store the instance created event
                InstanceEvent instanceEvent = new InstanceEvent
                {
                    AuthenticationLevel = requestContext.UserContext.AuthenticationLevel,
                    EventType = InstanceEventType.Created.ToString(),
                    InstanceId = instance.Id,
                    InstanceOwnerId = instanceOwnerId.ToString(),
                    UserId = requestContext.UserContext.UserId,
                    ProcessInfo = instance.Process,
                };

                Enum.TryParse<WorkflowStep>(instance.Process.CurrentTask.Name, out WorkflowStep currentStep);

                string redirectUrl = _workflowSI.GetUrlForCurrentState(Guid.Parse(instance.Id), startServiceModel.Org, startServiceModel.Service, currentStep);
                return Redirect(redirectUrl);
            }

            startServiceModel.PartyList = _authorization.GetPartyList(requestContext.UserContext.UserId)
               .Select(x => new SelectListItem
               {
                   Text = (x.PartyTypeName == PartyType.Person) ? x.SSN + " " + x.Name : x.OrgNumber + " " + x.Name,
                   Value = x.PartyId.ToString(),
               }).ToList();

            HttpContext.Response.Cookies.Append(_generalSettings.GetAltinnPartyCookieName, startServiceModel.PartyId.ToString());
            return View(startServiceModel);
        }

        /// <summary>
        /// Get the current state.
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="instanceGuid">The instance id.</param>
        /// <param name="reporteeId">The reportee id.</param>
        /// <returns>An api response containing the current ServiceState.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult GetCurrentState(string org, string app, Guid instanceGuid, int reporteeId)
        {
            return new ObjectResult(_workflowSI.GetCurrentState(instanceGuid, org, app, reporteeId));
        }

        /// <summary>
        /// Validate the model.
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="partyId">The partyId.</param>
        /// <param name="instanceGuid">The instanceGuid.</param>
        /// <returns>The api response.</returns>
        public async Task<IActionResult> ModelValidation(string org, string app, int partyId, Guid instanceGuid)
        {
            // Dependency Injection: Getting the Service Specific Implementation based on the app parameter data store
            // Will compile code and load DLL in to memory for AltinnCore
            IServiceImplementation serviceImplementation = _execution.GetServiceImplementation(org, app, false);

            // Create and populate the RequestContext object and make it available for the service implementation so
            // app developer can implement logic based on information about the request and the user performing
            // the request
            RequestContext requestContext = RequestHelper.GetRequestContext(Request.Query, instanceGuid);
            requestContext.UserContext = await _userHelper.GetUserContext(HttpContext);
            requestContext.Party = requestContext.UserContext.Party;
            requestContext.Form = Request.Form;

            // Get the serviceContext containing all metadata about current app.
            ServiceContext serviceContext = _execution.GetServiceContext(org, app, false);

            // Assign the Requestcontext and ViewBag to the serviceImplementation so
            // app developer can use the information in any of the service events that is called
            serviceImplementation.SetContext(requestContext, serviceContext, null, ModelState);

            // Set the platform services to the ServiceImplementation so the AltinnCore service can take
            // use of the platform services
            serviceImplementation.SetPlatformServices(_platformSI);
            Instance instance = await _instance.GetInstance(app, org, requestContext.UserContext.PartyId, instanceGuid);
            Guid.TryParse(instance.Data.Find(m => m.ElementType == FORM_ID).Id, out Guid dataId);

            // Getting the populated form data from disk
            dynamic serviceModel = _data.GetFormData(
                instanceGuid,
                serviceImplementation.GetServiceModelType(),
                org,
                app,
                requestContext.UserContext.PartyId,
                dataId);

            serviceImplementation.SetServiceModel(serviceModel);

            // Do Model Binding and update form data
            await TryUpdateModelAsync(serviceModel);

            // ServiceEvent : HandleValidationEvent
            // Perform Validation defined by the service developer.
            await serviceImplementation.RunServiceEvent(ServiceEventType.Validation);

            ApiResult apiResult = new ApiResult();
            ModelHelper.MapModelStateToApiResult(ModelState, apiResult, serviceContext);

            if (apiResult.Status.Equals(ApiStatusType.ContainsError))
            {
                Response.StatusCode = 202;
            }
            else
            {
                Response.StatusCode = 200;
            }

            return new ObjectResult(apiResult);
        }

        /// <summary>
        /// Method that receives the form attachment from runtime and saves it to designer disk.
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="partyId">The party id of the test user</param>
        /// <param name="instanceGuid">The instance guid</param>
        /// <param name="attachmentType">The attachment type id</param>
        /// <param name="attachmentName">The name of the attachment</param>
        /// <returns>The status of the upload and guid of attachment</returns>
        [HttpPost]
        [Authorize]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(REQUEST_SIZE_LIMIT)]
        public async Task<IActionResult> SaveFormAttachment(string org, string app, int partyId, Guid instanceGuid, string attachmentType, string attachmentName)
        {
            DataElement data = await _data.InsertBinaryData(org, app, partyId, instanceGuid, attachmentType, attachmentName, Request);
            Guid guid = Guid.Parse(data.Id);

            if (guid == Guid.Empty)
            {
                return StatusCode(500, $"Cannot store form attachment on instance {partyId}/{instanceGuid}");
            }

            return Ok(data);
        }

        /// <summary>
        /// Method that removes a form attachment from designer disk
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="partyId">The party id of the test user</param>
        /// <param name="instanceGuid">The instance guid</param>
        /// <param name="attachmentId">The attachment id</param>
        /// <returns>The status of the deletion</returns>
        [HttpPost]
        [Authorize]
        [DisableFormValueModelBinding]
        public IActionResult DeleteFormAttachment(string org, string app, int partyId, Guid instanceGuid, string attachmentId)
        {
            _data.DeleteBinaryData(org, app, partyId, instanceGuid, Guid.Parse(attachmentId));

            return Ok();
        }

        /// <summary>
        /// Method that gets metadata on form attachments form designer disk
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="partyId">The party id of the test user</param>
        /// <param name="instanceGuid">The instance guid</param>
        /// <returns>A list with attachments metadata ordered by attachmentType</returns>
        [HttpGet]
        [Authorize]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(REQUEST_SIZE_LIMIT)]
        public async Task<IActionResult> GetFormAttachments(string org, string app, int partyId, Guid instanceGuid)
        {
            List<AttachmentList> allAttachments = await _data.GetBinaryDataList(org, app, partyId, instanceGuid);

            return Ok(allAttachments);
        }

        private async Task<RequestContext> PopulateRequestContext(Guid instanceGuid)
        {
            // Create and populate the RequestContext object and make it available for the service implementation so
            // app developer can implement logic based on information about the request and the user performing
            // the request
            RequestContext requestContext = RequestHelper.GetRequestContext(Request.Query, instanceGuid);
            requestContext.UserContext = await _userHelper.GetUserContext(HttpContext);
            requestContext.Party = requestContext.UserContext.Party;
            if (Request.Method.Equals("post"))
            {
                requestContext.Form = Request.Form;
            }

            return requestContext;
        }

        private void PopulateViewBag(string org, string app, Guid instanceGuid, int? itemId, RequestContext requestContext, ServiceContext serviceContext, IPlatformServices platformServices)
        {
            ViewBag.RequestContext = requestContext;
            ViewBag.ServiceContext = serviceContext;
            ViewBag.Org = org;
            ViewBag.App = app;
            ViewBag.FormID = instanceGuid;
            ViewBag.PlatformServices = platformServices;

            if (itemId.HasValue)
            {
                ViewBag.ItemID = itemId.Value;
            }
        }
    }
}
