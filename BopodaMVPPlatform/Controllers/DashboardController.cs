﻿using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToProbeServer;
using BopodaMVPPlatform.Data;
using BopodaMVPPlatform.Models;
using BopodaMVPPlatform.Models.DashboardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BopodaMVPPlatform.Controllers
{
    [LimitPerMin]
    [Authorize]
    [Route("dashboard")]
    public class DashboardController : Controller
    {
        private readonly SitesService _sitesService;
        private readonly AppsContainer _appsContainer;
        private readonly FoldersService _foldersService;
        private readonly FilesService _filesService;
        private readonly MVPDbContext _dbContext;

        private Task<string> accesstoken => _appsContainer.AccessToken();

        public DashboardController(
            SitesService sitesService,
            AppsContainer appsContainer,
            FoldersService foldersService,
            FilesService filesService,
            MVPDbContext dbContext)
        {
            _sitesService = sitesService;
            _appsContainer = appsContainer;
            _foldersService = foldersService;
            _filesService = filesService;
            _dbContext = dbContext;
        }

        [Route("render-my-orgs")]
        public async Task<IActionResult> RenderMyOrgs()
        {
            var user = await GetCurrentUserAsync();
            var myOrgs = await _dbContext.UserRelationships
                .Where(t => t.UserId == user.Id)
                .Select(t => t.Organization)
                .ToListAsync();
            var model = new RenderMyOrgsViewModel
            {
                MyOrgs = myOrgs
            };
            return PartialView(model);
        }

        [Route("organizations")]
        public async Task<IActionResult> AllOrgs()
        {
            var user = await GetCurrentUserAsync();
            var myOrgs = await _dbContext.UserRelationships
                .Where(t => t.UserId == user.Id)
                .Select(t => t.Organization)
                .ToListAsync();
            if (!myOrgs.Any())
            {
                return RedirectToAction(nameof(CreateOrg));
            }
            var model = new AllOrgsViewModel(user)
            {
                MyOrgs = myOrgs
            };
            return View(model);
        }

        [Route("organizations/create")]
        public async Task<IActionResult> CreateOrg()
        {
            var user = await GetCurrentUserAsync();
            var model = new CreateOrgViewModel(user);
            return View(model);
        }

        [HttpPost]
        [Route("organizations/create")]
        public async Task<IActionResult> CreateOrg(CreateOrgViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (!ModelState.IsValid)
            {
                model.ModelStateValid = false;
                model.Recover(user);
                return View(model);
            }
            // Ensure no org with the same name.
            if (await _dbContext.Organizations.AnyAsync(t => t.DisplayName.ToLower() == model.OrganizationName.ToLower()))
            {
                ModelState.AddModelError("", $"There is already an organization with name: '{model.OrganizationName.ToLower()}'");
                model.ModelStateValid = false;
                model.Recover(user);
                return View(model);
            }
            // Ensure site can be created.
            try
            {
                await _sitesService.CreateNewSiteAsync(await accesstoken, model.OrganizationSiteName, true, true);
            }
            catch (AiurUnexceptedResponse e)
            {
                ModelState.AddModelError(string.Empty, e.Response.Message);
                model.ModelStateValid = false;
                model.Recover(user);
                return View(model);
            }
            // Create org.
            var newOrg = new Organization
            {
                DisplayName = model.OrganizationName,
                Description = model.OrganizationDescription,
                SiteName = model.OrganizationSiteName
            };
            _dbContext.Organizations.Add(newOrg);
            await _dbContext.SaveChangesAsync();
            var newRelation = new UserRelationship
            {
                OrganizationId = newOrg.Id,
                UserId = user.Id
            };
            _dbContext.UserRelationships.Add(newRelation);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(OrgHome), new OrgHomeAddressModel
            {
                OrgName = model.OrganizationName
            });
        }

        [Route("organizations/home/{OrgName}")]
        public async Task<IActionResult> OrgHome(OrgHomeAddressModel model)
        {
            return Json(model);
        }

        //[Route("Index")]
        //public async Task<IActionResult> Index()
        //{
        //    var user = await GetCurrentUserAsync();
        //    var sites = await _sitesService.ViewMySitesAsync(await accesstoken);
        //    if (string.IsNullOrEmpty(user.SiteName) || !sites.Sites.Any(t => t.SiteName == user.SiteName))
        //    {
        //        return RedirectToAction(nameof(CreateSite));
        //    }
        //    var model = new IndexViewModel(user)
        //    {
        //        SiteName = user.SiteName
        //    };
        //    return View(model);
        //}

        //[Route("CreateSite")]
        //public async Task<IActionResult> CreateSite()
        //{
        //    var user = await GetCurrentUserAsync();
        //    var sites = await _sitesService.ViewMySitesAsync(await accesstoken);
        //    if (!string.IsNullOrEmpty(user.SiteName) && sites.Sites.Any(t => t.SiteName == user.SiteName))
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    var model = new CreateSiteViewModel(user)
        //    {

        //    };
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("CreateSite")]
        //public async Task<IActionResult> CreateSite(CreateSiteViewModel model)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (!ModelState.IsValid)
        //    {
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //    try
        //    {
        //        await _sitesService.CreateNewSiteAsync(await accesstoken, model.SiteName, model.OpenToUpload, model.OpenToDownload);
        //        user.SiteName = model.SiteName;
        //        await _dbContext.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (AiurUnexceptedResponse e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Response.Message);
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //}

        //[Route("ViewFiles/{**path}")]
        //public async Task<IActionResult> ViewFiles(string path, bool justHaveUpdated)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (string.IsNullOrWhiteSpace(user.SiteName))
        //    {
        //        return RedirectToAction(nameof(CreateSite));
        //    }
        //    try
        //    {
        //        var data = await _foldersService.ViewContentAsync(await accesstoken, user.SiteName, path);
        //        var model = new ViewFilesViewModel(user)
        //        {
        //            Folder = data.Value,
        //            Path = path,
        //            SiteName = user.SiteName,
        //            JustHaveUpdated = justHaveUpdated
        //        };
        //        return View(model);
        //    }
        //    catch (AiurUnexceptedResponse e) when (e.Code == ErrorType.NotFound)
        //    {
        //        return NotFound();
        //    }
        //}

        //[Route("NewFolder/{**path}")]
        //public async Task<IActionResult> NewFolder(string path)
        //{
        //    var user = await GetCurrentUserAsync();
        //    var model = new NewFolderViewModel(user)
        //    {
        //        Path = path
        //    };
        //    return View(model);
        //}

        //[HttpPost]
        //[Route("NewFolder/{**path}")]
        //public async Task<IActionResult> NewFolder(NewFolderViewModel model)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (!ModelState.IsValid)
        //    {
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //    try
        //    {
        //        await _foldersService.CreateNewFolderAsync(await accesstoken, user.SiteName, model.Path, model.NewFolderName, false);
        //        return RedirectToAction(nameof(ViewFiles), new { path = model.Path });
        //    }
        //    catch (AiurUnexceptedResponse e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Response.Message);
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //}

        //[Route("DeleteFolder/{**path}")]
        //public async Task<IActionResult> DeleteFolder([FromRoute]string path)
        //{
        //    var user = await GetCurrentUserAsync();
        //    var model = new DeleteFolderViewModel(user)
        //    {
        //        Path = path
        //    };
        //    return View(model);
        //}

        //[HttpPost]
        //[Route("DeleteFolder/{**path}")]
        //public async Task<IActionResult> DeleteFolder(DeleteFolderViewModel model)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (!ModelState.IsValid)
        //    {
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //    try
        //    {
        //        await _foldersService.DeleteFolderAsync(await accesstoken, user.SiteName, model.Path);
        //        return RedirectToAction(nameof(ViewFiles), new { path = model.Path.DetachPath() });
        //    }
        //    catch (AiurUnexceptedResponse e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Response.Message);
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //}

        //[Route("DeleteFile/{**path}")]
        //public async Task<IActionResult> DeleteFile([FromRoute]string path)
        //{
        //    var user = await GetCurrentUserAsync();
        //    var model = new DeleteFileViewModel(user)
        //    {
        //        Path = path
        //    };
        //    return View(model);
        //}

        //[HttpPost]
        //[Route("DeleteFile/{**path}")]
        //public async Task<IActionResult> DeleteFile(DeleteFileViewModel model)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (!ModelState.IsValid)
        //    {
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //    try
        //    {
        //        await _filesService.DeleteFileAsync(await accesstoken, user.SiteName, model.Path);
        //        return RedirectToAction(nameof(ViewFiles), new { path = model.Path.DetachPath() });
        //    }
        //    catch (AiurUnexceptedResponse e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Response.Message);
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //}

        //[Route("Delete")]
        //public async Task<IActionResult> Delete()
        //{
        //    var user = await GetCurrentUserAsync();
        //    var model = new DeleteViewModel(user)
        //    {
        //        SiteName = user.SiteName
        //    };
        //    return View(model);
        //}

        //[Route("Delete")]
        //[HttpPost]
        //public async Task<IActionResult> Delete(DeleteViewModel model)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (!ModelState.IsValid)
        //    {
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //    try
        //    {
        //        await _sitesService.DeleteSiteAsync(await accesstoken, user.SiteName);
        //        user.SiteName = string.Empty;
        //        await _dbContext.SaveChangesAsync();
        //        return RedirectToAction(nameof(CreateSite));
        //    }
        //    catch (AiurUnexceptedResponse e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Response.Message);
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //}

        //[Route("Settings")]
        //public async Task<IActionResult> Settings(bool justHaveUpdated)
        //{
        //    var user = await GetCurrentUserAsync();
        //    var sites = await _sitesService.ViewMySitesAsync(await accesstoken);
        //    var hasASite = !string.IsNullOrEmpty(user.SiteName) && sites.Sites.Any(t => t.SiteName == user.SiteName);
        //    if (hasASite)
        //    {
        //        var siteDetail = await _sitesService.ViewSiteDetailAsync(await accesstoken, user.SiteName);
        //        var model = new SettingsViewModel(user)
        //        {
        //            SiteSize = siteDetail.Size,
        //            HasASite = true,
        //            JustHaveUpdated = justHaveUpdated,
        //            NewSiteName = siteDetail.Site.SiteName,
        //            OldSiteName = siteDetail.Site.SiteName,
        //            OpenToDownload = siteDetail.Site.OpenToDownload,
        //            OpenToUpload = siteDetail.Site.OpenToUpload
        //        };
        //        return View(model);
        //    }
        //    else
        //    {
        //        var model = new SettingsViewModel(user)
        //        {
        //            HasASite = false,
        //            JustHaveUpdated = justHaveUpdated
        //        };
        //        return View(model);
        //    }
        //}

        //[HttpPost]
        //[Route("Settings")]
        //public async Task<IActionResult> Settings(SettingsViewModel model)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (!ModelState.IsValid)
        //    {
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        return View(model);
        //    }
        //    try
        //    {
        //        var token = await _appsContainer.AccessToken();
        //        await _sitesService.UpdateSiteInfoAsync(token, model.OldSiteName, model.NewSiteName, model.OpenToUpload, model.OpenToDownload);
        //        user.SiteName = model.NewSiteName;
        //        await _dbContext.SaveChangesAsync();
        //        return RedirectToAction(nameof(DashboardController.Settings), "Dashboard", new { JustHaveUpdated = true });
        //    }
        //    catch (AiurUnexceptedResponse e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Response.Message);
        //        model.ModelStateValid = false;
        //        model.Recover(user);
        //        model.NewSiteName = model.OldSiteName;
        //        return View(model);
        //    }
        //}

        private async Task<MVPUser> GetCurrentUserAsync()
        {
            var id = User.Claims.SingleOrDefault(t => t.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            var name = User.Claims.SingleOrDefault(t => t.Type == "name")?.Value;
            var user = await _dbContext.Users.SingleOrDefaultAsync(t => t.Id == id);
            if (user == null)
            {
                var newUser = new MVPUser
                {
                    Id = id,
                    SiteName = string.Empty,
                };
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
            }
            return user;
        }
    }
}
