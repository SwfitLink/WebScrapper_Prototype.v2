﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
    public class Gpt3
    {
		[Key]
        public int Id { get; set; }
        public string Ingredient1 { get; set; }
		public string Ingredient2 { get; set; }
		public string Ingredient3 { get; set; }
		public string Ingredient4 { get; set; }
		public string Ingredient5 { get; set; }



	}
}
