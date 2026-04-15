using System;
using System.Collections.Generic;
using System.Linq;
using ECommerce.Application.DTOs.product;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Helpers;

public static class ImageHelper
{
    public static void AddImages(
        ProductSku sku,
        List<ProductImageCreateDto>? images,
        string altText)
    {
        if (images == null || images.Count == 0) return;

        for (int i = 0; i < images.Count; i++)
        {
            var img = images[i];
            sku.ProductImages.Add(new ProductImage
            {
                Sku = sku,
                ImageUrl = img.ImageUrl,
                ThumbnailUrl = img.ThumbnailUrl ?? img.ImageUrl,
                AltText = img.AltText ?? altText,
                DisplayOrder = img.DisplayOrder ?? i + 1,
                IsPrimary = (i == 0), // make first image primary
                IsDeleted = false,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Merges image updates without overwriting untouched images.
    /// - Id present + IsDeleted=true → soft-delete that image
    /// - Id present → update fields on existing image
    /// - No Id → add as new image
    /// </summary>
    public static void MergeImages(
        ProductSku sku,
        List<ProductImageUpdateDto> images,
        DateTime now,
        string altText)
    {
        if (images.Count == 0) return;

        var existingById = sku.ProductImages
            .Where(i => !i.IsDeleted)
            .ToDictionary(i => i.ImageId);

        ProductImage? newPrimary = null;

        foreach (var img in images)
        {
            if (img.Id.HasValue && existingById.TryGetValue(img.Id.Value, out var existing))
            {
                if (img.IsDeleted == true)
                {
                    existing.SoftDelete();
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(img.ImageUrl)) existing.ImageUrl = img.ImageUrl;
                if (img.ThumbnailUrl != null) existing.ThumbnailUrl = img.ThumbnailUrl;
                if (img.AltText != null) existing.AltText = img.AltText;
                if (img.IsPrimary == true) newPrimary = existing;
                if (img.IsPrimary.HasValue) existing.IsPrimary = img.IsPrimary.Value;
                if (img.DisplayOrder.HasValue) existing.DisplayOrder = img.DisplayOrder.Value;
                existing.UpdatedAt = now;
            }
            else if (!img.Id.HasValue && !string.IsNullOrWhiteSpace(img.ImageUrl))
            {
                var newImage = new ProductImage
                {
                    Sku = sku,
                    ImageUrl = img.ImageUrl,
                    ThumbnailUrl = img.ThumbnailUrl ?? img.ImageUrl,
                    AltText = img.AltText ?? altText,
                    DisplayOrder = img.DisplayOrder ?? sku.ProductImages.Count + 1,
                    IsPrimary = img.IsPrimary ?? false,
                    IsDeleted = false,
                    UpdatedAt = now
                };
                sku.ProductImages.Add(newImage);

                if (img.IsPrimary == true) newPrimary = newImage;
            }
        }

        // Enforce single primary: unset all others
        if (newPrimary != null)
        {
            foreach (var img in sku.ProductImages.Where(i => !i.IsDeleted && i != newPrimary))
            {
                img.IsPrimary = false;
            }
        }
    }
}