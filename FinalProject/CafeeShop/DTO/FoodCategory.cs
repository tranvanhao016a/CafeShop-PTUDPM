﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafee.DTO
{
    internal class FoodCategory
    {
        public int id { get; set; }
        public int idCate { get; set; }
        public byte[] icon { get; set; } 
        public string name { get; set; }
    }
}
