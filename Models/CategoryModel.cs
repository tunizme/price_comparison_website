﻿using System.ComponentModel.DataAnnotations;

namespace price_comparison.Models;

public class CategoryModel
{
    [Key]
    public int Id{get;set;}
    
    [Required, MinLength(4, ErrorMessage = "Nhập tên danh mục!!")]
    public string Name{get;set;}
    
    [Required, MinLength(4, ErrorMessage = "Nhập mô tả danh mục!!")]
    public string Description{get;set;}
    
    public string Slug{get;set;}
    
    [Required]
    public int Status{get;set;}
}