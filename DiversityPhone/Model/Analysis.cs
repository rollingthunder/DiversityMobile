﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using System.Data.Linq;


namespace DiversityPhone.Model
{
    [Table]
    public class Analysis
    {
        //Read-Only

        public Analysis()
        {
            LogUpdatedWhen = DateTime.Now;
        }

        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column]
        public String DisplayText { get; set; }

        [Column]
        public String Description { get; set; }

        [Column]
        public String MeasurementUnit { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }
    }
}
