CREATE TABLE `CmUser` (
    `Id`         VarChar(50)     NOT NULL PRIMARY KEY,
    `CreateBy`   VarChar(50)     NOT NULL,
    `CreateTime` DateTime        NOT NULL,
    `ModifyBy`   VarChar(50)     NULL,
    `ModifyTime` DateTime        NULL,
    `Version`    Long            NOT NULL,
    `Extension`  LongText        NULL,
    `AppId`      VarChar(50)     NOT NULL,
    `CompNo`     VarChar(50)     NOT NULL,
    `UserName`   VarChar(50)     NOT NULL,
    `Password`   VarChar(50)     NOT NULL,
    `OpenId`     VarChar(50)     NULL,
    `UnionId`    VarChar(50)     NULL,
    `NickName`   VarChar(50)     NULL,
    `Sex`        VarChar(50)     NULL,
    `Country`    VarChar(50)     NULL,
    `Province`   VarChar(50)     NULL,
    `City`       VarChar(50)     NULL,
    `AvatarUrl`  VarChar(250)    NULL,
    `Metier`     VarChar(50)     NULL,
    `Status`     VarChar(50)     NOT NULL,
    `Integral`   Long            NULL,
    `ContentQty` Long            NULL,
    `ReplyQty`   Long            NULL
)
GO

CREATE TABLE `CmCategory` (
    `Id`         VarChar(50)     NOT NULL PRIMARY KEY,
    `CreateBy`   VarChar(50)     NOT NULL,
    `CreateTime` DateTime        NOT NULL,
    `ModifyBy`   VarChar(50)     NULL,
    `ModifyTime` DateTime        NULL,
    `Version`    Long            NOT NULL,
    `Extension`  LongText        NULL,
    `AppId`      VarChar(50)     NOT NULL,
    `CompNo`     VarChar(50)     NOT NULL,
    `Type`       VarChar(50)     NOT NULL,
    `ParentId`   VarChar(50)     NOT NULL,
    `Code`       VarChar(50)     NOT NULL,
    `Name`       VarChar(50)     NOT NULL,
    `Icon`       VarChar(50)     NULL,
    `Sort`       Long            NOT NULL,
    `Enabled`    VarChar(50)     NOT NULL,
    `Note`       LongText        NULL
)
GO

CREATE TABLE `CmPost` (
    `Id`          VarChar(50)     NOT NULL PRIMARY KEY,
    `CreateBy`    VarChar(50)     NOT NULL,
    `CreateTime`  DateTime        NOT NULL,
    `ModifyBy`    VarChar(50)     NULL,
    `ModifyTime`  DateTime        NULL,
    `Version`     Long            NOT NULL,
    `Extension`   LongText        NULL,
    `AppId`       VarChar(50)     NOT NULL,
    `CompNo`      VarChar(50)     NOT NULL,
    `Type`        VarChar(50)     NOT NULL,
    `UserId`      VarChar(50)     NOT NULL,
    `CategoryId`  VarChar(50)     NULL,
    `Title`       VarChar(250)    NOT NULL,
    `Content`     LongText        NOT NULL,
    `Summary`     VarChar(500)    NULL,
    `Tags`        VarChar(200)    NULL,
    `Image`       VarChar(250)    NULL,
    `Files`       VarChar(250)    NULL,
    `Status`      VarChar(50)     NOT NULL,
    `PublishTime` DateTime        NULL,
    `ViewQty`     Long            NULL,
    `LikeQty`     Long            NULL,
    `ReplyQty`    Long            NULL,
    `RankNo`      Long            NULL
)
GO

CREATE TABLE `CmReply` (
    `Id`         VarChar(50)     NOT NULL PRIMARY KEY,
    `CreateBy`   VarChar(50)     NOT NULL,
    `CreateTime` DateTime        NOT NULL,
    `ModifyBy`   VarChar(50)     NULL,
    `ModifyTime` DateTime        NULL,
    `Version`    Long            NOT NULL,
    `Extension`  LongText        NULL,
    `AppId`      VarChar(50)     NOT NULL,
    `CompNo`     VarChar(50)     NOT NULL,
    `BizType`    VarChar(50)     NOT NULL,
    `BizId`      VarChar(50)     NOT NULL,
    `UserId`     VarChar(50)     NOT NULL,
    `Content`    LongText        NOT NULL,
    `ReplyTime`  DateTime        NOT NULL,
    `LikeQty`    Long            NULL,
    `ReplyQty`   Long            NULL
)
GO

CREATE TABLE `CmLog` (
    `Id`         VarChar(50)     NOT NULL PRIMARY KEY,
    `CreateBy`   VarChar(50)     NOT NULL,
    `CreateTime` DateTime        NOT NULL,
    `ModifyBy`   VarChar(50)     NULL,
    `ModifyTime` DateTime        NULL,
    `Version`    Long            NOT NULL,
    `Extension`  LongText        NULL,
    `AppId`      VarChar(50)     NOT NULL,
    `CompNo`     VarChar(50)     NOT NULL,
    `BizType`    VarChar(50)     NOT NULL,
    `LogType`    VarChar(50)     NOT NULL,
    `BizId`      VarChar(50)     NOT NULL,
    `UserId`     VarChar(50)     NOT NULL,
    `UserIP`     VarChar(50)     NULL
)
GO