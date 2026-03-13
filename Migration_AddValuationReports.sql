-- ============================================================
-- RELP-58: Add ValuationReports table
-- Run this script on an existing database that was set up
-- using Database.sql (i.e. without running EF migrations).
-- ============================================================

USE [RealEstateListingPlatform];
GO

-- Guard: only create if the table does not already exist
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ValuationReports'
)
BEGIN
    CREATE TABLE [dbo].[ValuationReports] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWID(),
        [UserId]            UNIQUEIDENTIFIER    NOT NULL,
        [ReportName]        NVARCHAR(200)       NULL,
        [PropertyType]      NVARCHAR(50)        NOT NULL,
        [TransactionType]   NVARCHAR(20)        NOT NULL,
        [AreaSqm]           DECIMAL(10, 2)      NOT NULL,
        [City]              NVARCHAR(100)       NOT NULL,
        [District]          NVARCHAR(100)       NOT NULL,
        [Ward]              NVARCHAR(100)       NULL,
        [AddressLine]       NVARCHAR(250)       NULL,
        [Notes]             NVARCHAR(500)       NULL,
        [EstimatedMinPrice] DECIMAL(18, 2)      NULL,
        [EstimatedAvgPrice] DECIMAL(18, 2)      NULL,
        [EstimatedMaxPrice] DECIMAL(18, 2)      NULL,
        [AvgPricePerSqm]    DECIMAL(18, 2)      NULL,
        [SampleCount]       INT                 NOT NULL    DEFAULT 0,
        [IsFallbackToCity]  BIT                 NOT NULL    DEFAULT 0,
        [MarketInsight]     NVARCHAR(1000)      NOT NULL    DEFAULT '',
        [CreatedAt]         DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_ValuationReports] PRIMARY KEY ([Id])
    );

    -- Index for fast per-user lookup
    CREATE INDEX [IX_ValuationReports_UserId]
        ON [dbo].[ValuationReports] ([UserId]);

    -- Foreign key to Users
    ALTER TABLE [dbo].[ValuationReports]
        WITH CHECK ADD CONSTRAINT [FK_ValuationReports_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE CASCADE;

    ALTER TABLE [dbo].[ValuationReports]
        CHECK CONSTRAINT [FK_ValuationReports_Users_UserId];

    PRINT 'Table ValuationReports created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ValuationReports already exists – skipped.';
END
GO
