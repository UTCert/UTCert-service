create database dev_utcert

USE [dev_utcert]
GO

/****** Object:  Table [dbo].[Users]    Script Date: 3/16/2024 9:16:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[StakeId] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[AvatarUri] [nvarchar](max) NULL,
	[Role] [int] NULL,
	[IsVerified] [bit] NULL,
	[IsDeleted] [bit] NULL,
	--[VerificationToken] [nvarchar](max) NULL,
	--[ResetToken] [nvarchar](max) NULL,
	--[ResetTokenExpires] [datetime] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

USE [dev_utcert]
GO

/****** Object:  Table [dbo].[Certificates]    Script Date: 3/16/2024 9:21:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Certificates](
	[Id] [uniqueidentifier] NOT NULL,
	[IssuerId] [uniqueidentifier] NOT NULL,
	[ReceiverId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[IpfsLink] [nvarchar](max) NULL,
	[Status] [tinyint] NULL,
	[ReceiverAddressWallet] [nvarchar](255) NULL,
	[ReceiverIdentityNumber] [nvarchar](255) NULL,
	[ReceiverName] [nvarchar](max) NULL,
	[ReceiverDoB] [date] NULL,
	[GraduationYear] [int] NULL,
	[Classification] [nvarchar](255) NULL,
	[StudyMode] [tinyint] NULL,
	[CreatedDate] [datetime] NULL,
	[SignedDate] [datetime] NULL,
	[SigningType] [tinyint] NULL,
	[SentDate] [datetime] NULL,
	[IsBanned] [bit] NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Certificates]  WITH CHECK ADD  CONSTRAINT [FK_Issuer_Certificates] FOREIGN KEY([IssuerId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Certificates] CHECK CONSTRAINT [FK_Issuer_Certificates]
GO

ALTER TABLE [dbo].[Certificates]  WITH CHECK ADD  CONSTRAINT [FK_Receiver_Certificates] FOREIGN KEY([ReceiverId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Certificates] CHECK CONSTRAINT [FK_Receiver_Certificates]
GO


USE [dev_utcert]
GO

/****** Object:  Table [dbo].[Contacts]    Script Date: 3/16/2024 9:21:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Contacts](
	[Id] [uniqueidentifier] NOT NULL,
	[IssuerId] [uniqueidentifier] NOT NULL,
	[ReceiverId] [uniqueidentifier] NOT NULL,
	[Status] [tinyint] NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Contacts]  WITH CHECK ADD  CONSTRAINT [FK_Issuer_Contacts] FOREIGN KEY([IssuerId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Contacts] CHECK CONSTRAINT [FK_Issuer_Contacts]
GO

ALTER TABLE [dbo].[Contacts]  WITH CHECK ADD  CONSTRAINT [FK_Receiver_Contacts] FOREIGN KEY([ReceiverId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Contacts] CHECK CONSTRAINT [FK_Receiver_Contacts]
GO


USE [dev_utcert]
GO


USE [dev_utcert]
GO

/****** Object:  Table [dbo].[RefreshTokens]    Script Date: 3/16/2024 9:22:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RefreshTokens](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Token] [nvarchar](max) NOT NULL,
	[Expires] [datetime] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedByIp] [nvarchar](max) NOT NULL,
	[Revoked] [datetime] NULL,
	[RevokedByIp] [nvarchar](max) NOT NULL,
	[ReplacedByToken] [nvarchar](max),
	[ReasonRevoked] [nvarchar](max),
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RefreshTokens]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO



ALTER TABLE Users
DROP COLUMN Verified;


ALTER TABLE Users
Add [Role] int;