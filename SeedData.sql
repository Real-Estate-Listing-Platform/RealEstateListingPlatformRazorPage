USE [RealEstateListingPlatform]
GO

-- =============================================
-- SEED DATA: Listings + ListingMedia
-- Keeps existing ListingPackages intact
-- =============================================

-- Clean up existing Listings & related data (CASCADE will handle ListingMedia, ListingViews, etc.)
DELETE FROM [dbo].[ListingBoosts]
DELETE FROM [dbo].[Leads]
DELETE FROM [dbo].[Favorites]
DELETE FROM [dbo].[Reports]
DELETE FROM [dbo].[ListingViews]
DELETE FROM [dbo].[ListingMedia]
DELETE FROM [dbo].[ListingPriceHistory]
DELETE FROM [dbo].[ListingTour360]
-- Remove snapshot FK first
UPDATE [dbo].[Listings] SET [PendingSnapshotId] = NULL
DELETE FROM [dbo].[ListingSnapshots]
DELETE FROM [dbo].[Listings]
GO

-- =============================================
-- INSERT LISTINGS (13 listings across different types/statuses)
-- Using existing User IDs:
--   Lister1: 1839f46c-b60e-4967-a77f-cdd4bd6b8415 (Đặng Minh Hoàng)
--   Lister2: f8e2cc8b-dcf4-4d79-b46a-35a8df579c94 (Đặng Quang Huy)
--   Lister3: dcf0a3c2-c271-4c7e-b58b-77ea89882494 (Phạm Hương Broker)
-- =============================================

-- 1) Căn hộ Vinhomes Grand Park 2PN - Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000001', N'dcf0a3c2-c271-4c7e-b58b-77ea89882494',
N'Bán căn hộ Vinhomes Grand Park 2PN, view sông thoáng mát',
N'Căn hộ 2 phòng ngủ tại Vinhomes Grand Park, diện tích 69m2, tầng cao view sông Tắc cực đẹp. Nội thất cao cấp, bàn giao đầy đủ. Tiện ích: hồ bơi, gym, công viên 36ha, trường học quốc tế.',
N'Sell', N'Apartment', CAST(3500000000.00 AS Decimal(18,2)),
N'Nguyễn Xiển', N'Long Thạnh Mỹ', N'Thủ Đức', N'Hồ Chí Minh', N'69m2', N'S5.02-12A',
10.840000, 106.840000, 2, 2, NULL, N'PinkBook', N'FullyFurnished', N'Southeast',
N'Published', CAST(N'2026-06-07T00:00:00' AS DateTime),
CAST(N'2026-02-15T08:30:00' AS DateTime), CAST(N'2026-02-15T08:30:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 2) Nhà phố Thủ Đức - Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000002', N'1839f46c-b60e-4967-a77f-cdd4bd6b8415',
N'Bán nhà phố 3 tầng mặt tiền đường Võ Văn Ngân, Thủ Đức',
N'Nhà phố 3 tầng mặt tiền đường Võ Văn Ngân, diện tích 5x20m, kết cấu 1 trệt 2 lầu. Vị trí kinh doanh đắc địa, gần ngã tư Thủ Đức. Sổ hồng riêng, công chứng ngay.',
N'Sell', N'House', CAST(8500000000.00 AS Decimal(18,2)),
N'Võ Văn Ngân', N'Trường Thọ', N'Thủ Đức', N'Hồ Chí Minh', N'100m2', N'125',
10.850000, 106.770000, 4, 3, 3, N'PinkBook', N'PartiallyFurnished', N'East',
N'Published', CAST(N'2026-06-01T00:00:00' AS DateTime),
CAST(N'2026-02-01T10:15:00' AS DateTime), CAST(N'2026-02-01T10:15:00' AS DateTime),
NULL, 1, 5, 0, 0, NULL)
GO

-- 3) Biệt thự Đà Nẵng - Published + Boosted
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000003', N'f8e2cc8b-dcf4-4d79-b46a-35a8df579c94',
N'Biệt thự nghỉ dưỡng view biển Mỹ Khê, Đà Nẵng',
N'Biệt thự sang trọng 3 tầng, diện tích 300m2, view biển Mỹ Khê trực diện. Hồ bơi riêng, sân vườn rộng, nội thất nhập khẩu Ý. Khu vực an ninh 24/7, cách biển 200m.',
N'Sell', N'Villa', CAST(15000000000.00 AS Decimal(18,2)),
N'Võ Nguyên Giáp', N'Phước Mỹ', N'Sơn Trà', N'Thành phố Đà Nẵng', N'300m2', N'88',
16.060000, 108.240000, 5, 4, 3, N'RedBook', N'FullyFurnished', N'East',
N'Published', CAST(N'2026-07-01T00:00:00' AS DateTime),
CAST(N'2026-01-20T14:00:00' AS DateTime), CAST(N'2026-03-01T09:00:00' AS DateTime),
NULL, 1, 10, 1, 1, NULL)
GO

-- 4) Cho thuê phòng trọ - Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000004', N'dcf0a3c2-c271-4c7e-b58b-77ea89882494',
N'Cho thuê phòng trọ cao cấp gần ĐH FPT, có nội thất',
N'Phòng trọ mới xây, diện tích 25m2, có gác lửng. Nội thất đầy đủ: máy lạnh, tủ quần áo, bàn học, giường. WC riêng, ban công thoáng. Gần ĐH FPT, chợ, siêu thị.',
N'Rent', N'Room', CAST(4500000.00 AS Decimal(18,2)),
N'Lê Văn Việt', N'Tăng Nhơn Phú', N'Thủ Đức', N'Hồ Chí Minh', N'25m2', N'45/12',
10.840000, 106.790000, 1, 1, NULL, NULL, N'FullyFurnished', N'South',
N'Published', CAST(N'2026-05-15T00:00:00' AS DateTime),
CAST(N'2026-02-20T09:30:00' AS DateTime), CAST(N'2026-02-20T09:30:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 5) Căn hộ cho thuê Masteri - Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000005', N'1839f46c-b60e-4967-a77f-cdd4bd6b8415',
N'Cho thuê căn hộ Masteri Thảo Điền 3PN, full nội thất',
N'Căn hộ 3 phòng ngủ tại Masteri Thảo Điền, tầng 25, view sông Sài Gòn. Diện tích 93m2, nội thất cao cấp. Tiện ích đầy đủ: hồ bơi, gym, sauna, BBQ area.',
N'Rent', N'Apartment', CAST(25000000.00 AS Decimal(18,2)),
N'Xa lộ Hà Nội', N'Thảo Điền', N'Thủ Đức', N'Hồ Chí Minh', N'93m2', NULL,
10.800000, 106.740000, 3, 2, NULL, N'PinkBook', N'FullyFurnished', N'Northeast',
N'Published', CAST(N'2026-06-20T00:00:00' AS DateTime),
CAST(N'2026-02-25T11:00:00' AS DateTime), CAST(N'2026-02-25T11:00:00' AS DateTime),
NULL, 1, 5, 0, 0, NULL)
GO

-- 6) Đất nền Hòa Xuân - Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000006', N'f8e2cc8b-dcf4-4d79-b46a-35a8df579c94',
N'Bán đất nền khu đô thị Hòa Xuân, Đà Nẵng',
N'Lô đất nền diện tích 100m2 (5x20m), mặt tiền đường 7.5m, khu đô thị Hòa Xuân. Hạ tầng hoàn thiện, gần trường học, chợ, công viên. Sổ đỏ chính chủ.',
N'Sell', N'Land', CAST(4200000000.00 AS Decimal(18,2)),
N'Đường Hòa Xuân', N'Hòa Xuân', N'Cẩm Lệ', N'Thành phố Đà Nẵng', N'100m2', NULL,
16.020000, 108.210000, NULL, NULL, NULL, N'RedBook', NULL, N'North',
N'Published', CAST(N'2026-07-15T00:00:00' AS DateTime),
CAST(N'2026-02-10T15:45:00' AS DateTime), CAST(N'2026-02-10T15:45:00' AS DateTime),
NULL, 1, 5, 0, 0, NULL)
GO

-- 7) Nhà nguyên căn cho thuê - Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000007', N'dcf0a3c2-c271-4c7e-b58b-77ea89882494',
N'Cho thuê nhà nguyên căn đường Dỗ Xuân Hợp, 4 phòng ngủ',
N'Nhà nguyên căn 1 trệt 2 lầu, diện tích 80m2, 4PN 3WC. Có sân để xe hơi, ban công thoáng. Khu vực an ninh, gần trường học, bệnh viện.',
N'Rent', N'House', CAST(18000000.00 AS Decimal(18,2)),
N'Dỗ Xuân Hợp', N'Phước Long B', N'Thủ Đức', N'Hồ Chí Minh', N'80m2', N'78/5',
10.830000, 106.780000, 4, 3, 2, N'PinkBook', N'Unfurnished', N'West',
N'Published', CAST(N'2026-05-01T00:00:00' AS DateTime),
CAST(N'2026-01-28T16:20:00' AS DateTime), CAST(N'2026-01-28T16:20:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 8) Căn hộ PendingReview
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000008', N'1839f46c-b60e-4967-a77f-cdd4bd6b8415',
N'Bán căn hộ Sunwah Pearl 1PN, Bình Thạnh',
N'Căn hộ 1 phòng ngủ tầng 18, diện tích 56m2, view Landmark 81. Nội thất cơ bản từ CĐT. Tiện ích: hồ bơi tràn viền, sky garden, gym.',
N'Sell', N'Apartment', CAST(4800000000.00 AS Decimal(18,2)),
N'Nguyễn Hữu Cảnh', N'22', N'Bình Thạnh', N'Hồ Chí Minh', N'56m2', NULL,
10.790000, 106.720000, 1, 1, NULL, N'PinkBook', N'PartiallyFurnished', N'Northwest',
N'PendingReview', CAST(N'2026-06-10T00:00:00' AS DateTime),
CAST(N'2026-03-05T07:00:00' AS DateTime), CAST(N'2026-03-05T07:00:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 9) Villa Rejected
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000009', N'f8e2cc8b-dcf4-4d79-b46a-35a8df579c94',
N'Biệt thự Euro Village, Sơn Trà, Đà Nẵng',
N'Biệt thự 2 mặt tiền tại Euro Village, diện tích 250m2. View sông Hàn tuyệt đẹp, gần cầu Rồng.',
N'Sell', N'Villa', CAST(22000000000.00 AS Decimal(18,2)),
N'Đường Euro Village', N'An Hải Bắc', N'Sơn Trà', N'Thành phố Đà Nẵng', N'250m2', N'15',
16.070000, 108.230000, 5, 5, 3, N'SaleContract', N'FullyFurnished', N'South',
N'Rejected', NULL,
CAST(N'2026-02-28T12:00:00' AS DateTime), CAST(N'2026-03-01T08:30:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 10) Nhà Draft
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000010', N'1839f46c-b60e-4967-a77f-cdd4bd6b8415',
N'Nhà cấp 4 hẻm xe hơi Lê Văn Việt',
N'Nhà cấp 4 diện tích 60m2, hẻm xe hơi 6m, gần Vincom Lê Văn Việt.',
N'Sell', N'House', CAST(3200000000.00 AS Decimal(18,2)),
N'Lê Văn Việt', N'Tăng Nhơn Phú A', N'Thủ Đức', N'Hồ Chí Minh', N'60m2', N'200/15',
10.850000, 106.790000, 2, 1, 1, N'Waiting', N'Unfurnished', N'North',
N'Draft', NULL,
CAST(N'2026-03-06T20:00:00' AS DateTime), CAST(N'2026-03-06T20:00:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 11) Căn hộ Expired
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000011', N'dcf0a3c2-c271-4c7e-b58b-77ea89882494',
N'Căn hộ The Sun Avenue 2PN, quận 2',
N'Căn hộ 76m2 tầng 15, nội thất đầy đủ, gần Metro, tiện di chuyển.',
N'Rent', N'Apartment', CAST(12000000.00 AS Decimal(18,2)),
N'Mai Chí Thọ', N'An Phú', N'Thủ Đức', N'Hồ Chí Minh', N'76m2', NULL,
10.770000, 106.750000, 2, 2, NULL, N'PinkBook', N'FullyFurnished', N'East',
N'Expired', CAST(N'2026-02-01T00:00:00' AS DateTime),
CAST(N'2025-12-01T10:00:00' AS DateTime), CAST(N'2026-02-01T00:00:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- 12) Đất nền Published
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000012', N'1839f46c-b60e-4967-a77f-cdd4bd6b8415',
N'Bán lô đất mặt tiền Nguyễn Duy Trinh, Long Trường',
N'Lô đất mặt tiền đường Nguyễn Duy Trinh 120m2 (6x20m). Đường rộng 30m, phù hợp xây biệt thự hoặc kinh doanh.',
N'Sell', N'Land', CAST(7800000000.00 AS Decimal(18,2)),
N'Nguyễn Duy Trinh', N'Long Trường', N'Thủ Đức', N'Hồ Chí Minh', N'120m2', NULL,
10.820000, 106.810000, NULL, NULL, NULL, N'RedBook', NULL, N'Southeast',
N'Published', CAST(N'2026-06-30T00:00:00' AS DateTime),
CAST(N'2026-02-18T13:30:00' AS DateTime), CAST(N'2026-02-18T13:30:00' AS DateTime),
NULL, 1, 5, 0, 0, NULL)
GO

-- 13) Nhà nguyên căn Hidden
INSERT [dbo].[Listings] ([Id],[ListerId],[Title],[Description],[TransactionType],[PropertyType],[Price],[StreetName],[Ward],[District],[City],[Area],[HouseNumber],[Latitude],[Longitude],[Bedrooms],[Bathrooms],[Floors],[LegalStatus],[FurnitureStatus],[Direction],[Status],[ExpirationDate],[CreatedAt],[UpdatedAt],[UserPackageId],[IsFreeListingorder],[MaxPhotos],[AllowVideo],[IsBoosted],[PendingSnapshotId])
VALUES (N'A0000001-0001-0001-0001-000000000013', N'f8e2cc8b-dcf4-4d79-b46a-35a8df579c94',
N'Cho thuê nhà nguyên căn đường Nguyễn Xiển, gần Vinhomes',
N'Nhà 1 trệt 1 lầu, 3PN, có sân trước rộng. Gần Vinhomes Grand Park, trường học.',
N'Rent', N'House', CAST(15000000.00 AS Decimal(18,2)),
N'Nguyễn Xiển', N'Long Thạnh Mỹ', N'Thủ Đức', N'Hồ Chí Minh', N'75m2', N'300/8',
10.835000, 106.835000, 3, 2, 2, N'PinkBook', N'PartiallyFurnished', N'West',
N'Hidden', NULL,
CAST(N'2026-02-05T17:00:00' AS DateTime), CAST(N'2026-03-01T10:00:00' AS DateTime),
NULL, 0, 5, 0, 0, NULL)
GO

-- =============================================
-- INSERT LISTING MEDIA (images for each listing)
-- Images use generated files copied to /uploads/listings/
-- =============================================

-- Listing 1: Căn hộ Vinhomes (3 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000001', N'A0000001-0001-0001-0001-000000000001', N'image', N'/uploads/listings/apartment_modern_1_1772852489427.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000002', N'A0000001-0001-0001-0001-000000000001', N'image', N'/uploads/listings/apartment_interior_1_1772852543084.png', 1)
GO

-- Listing 2: Nhà phố Thủ Đức (2 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000003', N'A0000001-0001-0001-0001-000000000002', N'image', N'/uploads/listings/townhouse_1_1772852504846.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000004', N'A0000001-0001-0001-0001-000000000002', N'image', N'/uploads/listings/house_traditional_1_1772852559825.png', 1)
GO

-- Listing 3: Biệt thự Đà Nẵng (2 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000005', N'A0000001-0001-0001-0001-000000000003', N'image', N'/uploads/listings/villa_luxury_1_1772852521618.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000006', N'A0000001-0001-0001-0001-000000000003', N'image', N'/uploads/listings/apartment_interior_1_1772852543084.png', 1)
GO

-- Listing 4: Phòng trọ (1 image)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000007', N'A0000001-0001-0001-0001-000000000004', N'image', N'/uploads/listings/apartment_interior_1_1772852543084.png', 0)
GO

-- Listing 5: Masteri Thảo Điền (2 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000008', N'A0000001-0001-0001-0001-000000000005', N'image', N'/uploads/listings/apartment_modern_1_1772852489427.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000009', N'A0000001-0001-0001-0001-000000000005', N'image', N'/uploads/listings/apartment_interior_1_1772852543084.png', 1)
GO

-- Listing 6: Đất nền Hòa Xuân (1 image)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000010', N'A0000001-0001-0001-0001-000000000006', N'image', N'/uploads/listings/land_plot_1_1772852574850.png', 0)
GO

-- Listing 7: Nhà nguyên căn Dỗ Xuân Hợp (2 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000011', N'A0000001-0001-0001-0001-000000000007', N'image', N'/uploads/listings/house_traditional_1_1772852559825.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000012', N'A0000001-0001-0001-0001-000000000007', N'image', N'/uploads/listings/townhouse_1_1772852504846.png', 1)
GO

-- Listing 8: Sunwah Pearl (1 image)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000013', N'A0000001-0001-0001-0001-000000000008', N'image', N'/uploads/listings/apartment_modern_1_1772852489427.png', 0)
GO

-- Listing 9: Euro Village (2 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000014', N'A0000001-0001-0001-0001-000000000009', N'image', N'/uploads/listings/villa_luxury_1_1772852521618.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000015', N'A0000001-0001-0001-0001-000000000009', N'image', N'/uploads/listings/apartment_interior_1_1772852543084.png', 1)
GO

-- Listing 10: Nhà cấp 4 Draft (1 image)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000016', N'A0000001-0001-0001-0001-000000000010', N'image', N'/uploads/listings/house_traditional_1_1772852559825.png', 0)
GO

-- Listing 11: The Sun Avenue Expired (2 images)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000017', N'A0000001-0001-0001-0001-000000000011', N'image', N'/uploads/listings/apartment_modern_1_1772852489427.png', 0)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000018', N'A0000001-0001-0001-0001-000000000011', N'image', N'/uploads/listings/apartment_interior_1_1772852543084.png', 1)
GO

-- Listing 12: Đất Nguyễn Duy Trinh (1 image)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000019', N'A0000001-0001-0001-0001-000000000012', N'image', N'/uploads/listings/land_plot_1_1772852574850.png', 0)
GO

-- Listing 13: Nhà nguyên căn Hidden (1 image)
INSERT [dbo].[ListingMedia] ([Id],[ListingId],[MediaType],[Url],[SortOrder]) VALUES (N'B0000001-0001-0001-0001-000000000020', N'A0000001-0001-0001-0001-000000000013', N'image', N'/uploads/listings/townhouse_1_1772852504846.png', 0)
GO

PRINT 'Seed data inserted successfully!'
PRINT '13 Listings created with 20 ListingMedia images'
PRINT 'ListingPackages preserved (not modified)'
GO
