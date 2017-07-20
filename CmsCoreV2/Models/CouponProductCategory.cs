﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CmsCoreV2.Models
{
    public class CouponProductCategory
    {
        public long ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public long CouponId { get; set; }
        public Coupon Coupon { get; set; }
    }
}
