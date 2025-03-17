﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Models
{
    public class AssignmentFile : BaseEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string FileExtension { get; set; }
        public DropboxFolders DropboxFolder { get; set; }
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }
    }
}