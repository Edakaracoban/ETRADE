﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Entities
{
    [Table("Image")]
    public class Image
    {
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; } //imageurl zorunlu
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
