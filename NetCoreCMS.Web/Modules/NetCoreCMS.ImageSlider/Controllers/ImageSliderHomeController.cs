﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreCMS.Framework.Core.Mvc.Controllers;
using NetCoreCMS.Framework.Core.Services;
using NetCoreCMS.Framework.Themes;
using NetCoreCMS.Framework.Utility;
using NetCoreCMS.ImageSlider.Models;
using NetCoreCMS.ImageSlider.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCoreCMS.ImageSlider.Controllers
{

    [Authorize(Roles = "SuperAdmin,Administrator,Editor")]
    [AdminMenu(Name = "Image Slider", IconCls = "", Order = 100)]
    public class ImageSliderHomeController : NccController
    {
        #region Initialization
        private NccSettingsService _nccSettingsService;
        private NccImageSliderService _nccImageSliderService;

        private NccImageSliderSettings nccImageSliderSettings;
        private List<NccImageSliderItem> nccImageSliderItemList = new List<NccImageSliderItem>();

        public ImageSliderHomeController(NccSettingsService nccSettingsService, ILoggerFactory factory, NccImageSliderService nccImageSliderService)
        {
            _logger = factory.CreateLogger<ImageSliderHomeController>();
            nccImageSliderSettings = new NccImageSliderSettings();

            _nccSettingsService = nccSettingsService;
            _nccImageSliderService = nccImageSliderService;
            try
            {
                var tempSettings = _nccSettingsService.GetByKey("NccImageSlider_Settings");
                if (tempSettings != null)
                {
                    nccImageSliderSettings = JsonConvert.DeserializeObject<NccImageSliderSettings>(tempSettings.Value);
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
        #endregion

        #region Admin Panel
        [AdminMenuItem(Name = "Slider Manage", Url = "/ImageSliderHome/Manage", IconCls = "", Order = 1)]
        public ActionResult Manage()
        {
            var itemList = _nccImageSliderService.LoadAll().OrderByDescending(x => x.Id).ToList(); ;
            return View(itemList);
        }


        [AdminMenuItem(Name = "New Slider", Url = "/ImageSliderHome/CreateEdit", Order = 2)]
        public ActionResult CreateEdit(long Id = 0)
        {
            NccImageSlider item = new NccImageSlider();

            if (Id > 0)
            {
                item = _nccImageSliderService.Get(Id);
                if (item.ImageItems != null)
                {
                    item.ImageItems = item.ImageItems.OrderBy(x => x.Order).ToList();
                }
            }
            return View(item);
        }

        [HttpPost]
        public ActionResult CreateEdit(NccImageSlider model, string save, string[] itemPath, string[] description)
        {
            bool isSuccess = false;
            ViewBag.MessageType = "ErrorMessage";
            ViewBag.Message = "Error occoured. Please fill up all field correctly.";

            if (ModelState.IsValid)
            {
                //unique name check
                model.Name = model.Name.Trim();
                var itemCount = _nccImageSliderService.LoadAll(true, 0, model.Name).Where(x => x.Id != model.Id).ToList().Count();
                if (itemCount > 0)
                {
                    ViewBag.Message = "Duplicate name found.";
                }
                else
                {
                    model.ImageItems = new List<NccImageSliderItem>();
                    for (int i = 0; i < itemPath.Count(); i++)
                    {
                        var tempItemPath = string.IsNullOrEmpty(itemPath[i]) ? "" : itemPath[i];
                        var tempDescription = string.IsNullOrEmpty(description[i]) ? "" : description[i];

                        model.ImageItems.Add(new NccImageSliderItem() { Name = "Name_" + tempItemPath.Replace("/", "_"), Path = tempItemPath, Description = tempDescription, Order = i });
                    }

                    if (model.Id > 0)
                    {
                        _nccImageSliderService.Update(model);
                        isSuccess = true;
                        ViewBag.MessageType = "SuccessMessage";
                        ViewBag.Message = "Data updated successfull.";
                    }
                    else
                    {
                        _nccImageSliderService.Save(model);
                        isSuccess = true;
                        ViewBag.MessageType = "SuccessMessage";
                        ViewBag.Message = "Data saved successfull.";
                    }
                }
            }

            if (isSuccess == true && save == "Save")
            {
                return RedirectToAction("Manage");
            }
            return View(model);
        }

        public ActionResult Delete(long Id)
        {
            NccImageSlider item = _nccImageSliderService.Get(Id);            
            return View(item);
        }

        [HttpPost]
        public ActionResult Delete(long Id, int status)
        {
            _nccImageSliderService.DeletePermanently(Id);
            ViewBag.MessageType = "SuccessMessage";
            ViewBag.Message = "Item deleted successful";
            return RedirectToAction("Manage");
        }
        #endregion

        #region User Panel
        #endregion
    }
}