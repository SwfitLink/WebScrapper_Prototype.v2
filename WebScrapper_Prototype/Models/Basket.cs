﻿using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
	public class Basket
	{
		[Key]
		public int Id { get; set; }
		public string UserId { get; set; }
		public int ProductKey { get; set; }
		public DateTime createdAt { get; set; }

	}
}
