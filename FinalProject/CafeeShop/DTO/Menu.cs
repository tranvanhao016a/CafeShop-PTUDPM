﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafee.DTO
{
    internal class Menu
    {
        public int id { get; set; }
        public float totalPrice { get; set; }
        public float price { get; set; }
        public int count { get; set; }
        public string foodName { get; set; }
    }
}
