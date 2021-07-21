﻿using FluentValidation;
using GenericApi.Bl.Dto;
using GenericApi.Bl.Validations;
using GenericApi.Model.Entities;
using GenericApi.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericApi.Controllers
{
    public class DocumentController : BaseController<Document, DocumentDto>
    {
        public DocumentController(IDocumentService service) : base(service)
        {
        }
    }
}
