﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RNN.Models
{
    public class Article : Entry
    {
        public Title Title { get; set; }
        public int? TitleId { get; set; }
        public string Paragraph { get; set; }
        public string Img { get; set; }
        public string Body { get; set; }
        public int? AuthorId { get; set; }
        public Author Author { get; set; }
        public int Rank { get; set; }
        public int Width { get; set; }
    }
}
