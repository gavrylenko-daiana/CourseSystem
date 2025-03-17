﻿using Core.Models;
using System.ComponentModel.DataAnnotations;

namespace UI.ViewModels
{
    public class CheckAnswerViewModel
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }

        [Required(ErrorMessage = "Grade is required")]
        [Display(Name = "Grade")]
        [Range(0, 100)]
        public int Grade { get; set; }
        public bool IsChecked { get; set; }
        public List<AssignmentAnswer> AssignmentAnswers { get; set; }
    }
}
