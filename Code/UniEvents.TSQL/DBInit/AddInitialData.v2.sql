SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ( NULL, 'Solar System', NULL, NULL, NULL, NULL, 'Milky Way', NULL, NULL, 'Our Solar System');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.[Name] = 'Solar System'), 'Earth', NULL, NULL, NULL, NULL, 'Solar System', NULL, NULL, 'Planet');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.[Name] = 'Earth'), 'North America', NULL, NULL, NULL, NULL, 'Earth', 54526000, -105255100, 'Continent');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.[Name] = 'North America'), 'United States of America', NULL, NULL, NULL, NULL, 'USA', 37090200, -95712900, 'Murica');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.CountryRegion = 'USA'), 'Nebraska', NULL, NULL, 'Nebraska', NULL, 'USA', 41492500, -99901800, 'Home of the Huskers');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.AdminDistrict = 'Nebraska'), NULL, NULL, 'Lincoln', 'Nebraska', NULL, 'USA', 40825800, -96685200, 'City of Lincoln Nebraska');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'UNL', '1400R Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40824310, -96698840, 'University of Nebraska at Lincoln');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'Love Library', '14th & R Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40818010, -96704760, NULL);

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'Avery Hall', '1144 T Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40819460, -96706640, NULL);

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'College of Business', '730 North 14th Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40820180, -96703390, 'University of Nebraska-Lincoln College of Business');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'Memorial Stadium', 'One Memorial Stadium Drive', 'Lincoln', 'Nebraska', 68588, 'USA', -96707830, -96707830, 'Home field of Nebraska Cornhuskers holding over 85,000 football fans with a history dating to 1922.');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.AdminDistrict ='Nebraska'), NULL, NULL, 'Columbus', 'Nebraska', NULL, 'USA', 41429734, -97368375, null);

GO


INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('No', 'Can''t attend');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Maybe', 'Not sure yet');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Stop By', 'I''ll Stop By');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Later', 'I''ll be late');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Attending', 'I''ll be there!');
GO


INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('Birthday', NULL);
INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('Party', NULL);
INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('Sports', NULL);
INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('ImpendingDoom', 'An event you''re dreading.');
GO

INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Football', 'American Football');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Soccer', 'Foreign Football');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Study', 'Informal Study Session');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Basketball-Game', 'Basketball Game');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('School-Sponsored', 'Sponsored by UNL');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('FREE FOOD', 'Free food');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('TV-Binge', 'Watch a TV Show');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Club', 'Sponsored by a Club');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Newbie', 'Beginner who needs guidance');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Rally', 'Gathering of People in Protest');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Concert', 'Music concert');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Free', 'Doesn’t cost anything');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Paid', 'Costs money');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Greek Life', 'Sponsored by Fraternity/Sorority');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Off-Campus', 'Takes place off campus');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('On-Campus', 'Takes place on campus');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('East-Campus', 'Takes place on East Campus');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Lit', 'This event will be lit');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Help', 'Pls help');
GO

--INSERT INTO [dbo].[Accounts] (UserName, DisplayName, PasswordHash, LocationID, IsGroup, ) VALUES 
--('Guest', 'Guest', null, (select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'North America'),
--	0);

--DECLARE	@AccountID BIGINT
--EXEC	[dbo].[sp_Account_Create]
--		@AccountID = @AccountID OUTPUT,
--		@Location = (SELECT TOP 1 locs.LocationID FROM [dbo].Locations AS locs WHERE locs.Name = 'North America'),
--		@Password = N'Guest',
--		@UserName = N'Guest',
--		@FirstName = N'Guest',
--		@LastName = N'User',
--		@SchoolEmail = N'test@hustkers.unl.edu',
--		@ContactEmail = N'test@gmail.com',
--		@PhoneNumber = N'402 555 5555'
--GO


--INSERT INTO [dbo].AccountDetails(AccountID, LocationID, FirstName, LastName, ContactEmail, PhoneNumber, [Description]) 
--	VALUES (
--		(select top 1 accts.AccountID from [dbo].Accounts as accts where accts.UserName = 'Guest'), 
--		(select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'North America'), 
--		null, null, null, null, 'Standard Guest Account');
--GO

--INSERT INTO [dbo].[Accounts] (UserName, DisplayName, PasswordHash, IsGroup) VALUES ('DerekZiembaTest', 'DerekTest', Cast('1C61624E80B8A7404A4BEF0398F5317F877FD69CFDC1B4B31B43C12F11FA2356' AS BINARY(256)), 0);
--INSERT INTO [dbo].AccountDetails(AccountID, LocationID, FirstName, LastName, ContactEmail, PhoneNumber, [Description]) 
--	VALUES (
--		(select top 1 accts.AccountID from [dbo].Accounts as accts where accts.UserName = 'DerekZiembaTest'), 
--		(select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'North America'), 
--		'Derek', 'Ziemba', 'iintendtocheckthisintogithubsono@gmail.com', '(402)555-1234', 'Dummy Account');
--GO


--INSERT INTO [dbo].[EventFeed]([AccountID], [LocationID], [EventTypeID], [DateStart], [DateEnd], [Title], [Caption])	
--	VALUES  (
--		(select top 1 accts.AccountID from [dbo].Accounts as accts where accts.UserName = 'DerekZiembaTest'), 
--		(select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'College of Business' AND locs.ParentLocationID = (select top 1 l2.LocationID from [dbo].Locations as l2 where l2.Name = 'UNL')), 
--		(select top 1 et.EventTypeID from [dbo].EventTypes as et where et.Name = 'ImpendingDoom'),
--		CONVERT(datetime,'2018-3-22 15:30') AT TIME ZONE 'Central Standard Time', 
--		CONVERT(datetime, '2018-3-22 16:20') AT TIME ZONE 'Central Standard Time', 'Due date of this assignment', 'oh boy');

--INSERT INTO [dbo].EventRSVPs([EventID], [AccountID], [RSVPTypeId])
--	VALUES (
--		(select top 1 ef.EventID from [dbo].EventFeed as ef where ef.DateStart > '2018-3-22' AND ef.DateEnd < '2018-3-23' AND ef.Title like '%Due Date%'),
--		(select top 1 ad.AccountID from [dbo].AccountDetails as ad where ad.FirstName = 'Derek' AND ad.LastName = 'Ziemba'),
--		(select top 1 rsvp.RSVPTypeID from [dbo].RSVPTypes as rsvp where rsvp.Name = 'Attending'));

--GO

