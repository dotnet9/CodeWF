namespace CodeWF.Admin.Helpers;

class ModuleHelper
{
    internal static void InitAppModules()
    {
        Config.OnAddModule = modules =>
        {
            var baseData = modules.FirstOrDefault(m => m.Name == "基础数据");
            modules.Add(GetBdUserList(baseData.Id));

            var document = GetModule("Contents", "内容管理", "file-search", ModuleType.Menu.ToString(), 2);
            modules.Add(document);
            modules.Add(GetDmUpdateLogList(document.Id));
            modules.Add(GetDmDocumentList(document.Id));
            modules.Add(GetDmArticleList(document.Id));

            var interact = GetModule("Interacts", "交流管理", "read", ModuleType.Menu.ToString(), 3);
            modules.Add(interact);
            modules.Add(GetImCategoryList(interact.Id));
            modules.Add(GetImPostList(interact.Id));
            modules.Add(GetImReplyList(interact.Id));
            modules.Add(GetImLogList(interact.Id));
        };
    }

    private static SysModule GetModule(string code, string name, string icon, string target, int sort)
    {
        return new SysModule { ParentId = "0", Code = code, Name = name, Icon = icon, Target = target, Sort = sort, Enabled = true };
    }

    #region 基础数据
    private static SysModule GetBdUserList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "BdUserList",
            Name = "网站用户",
            Icon = "user",
            Description = "查询和维护系统注册用户信息。",
            Target = ModuleType.Custom.ToString(),
            Url = "/bds/users",
            Sort = 3,
            Enabled = true,
            EntityData = @"用户信息|CmUser
账号|UserName|Text|50|Y
密码|Password|Text|50|Y
微信ID|OpenId|Text|50
微信ID|UnionId|Text|50
昵称|NickName|Text|50
性别|Sex|Text|50
国家|Country|Text|50
省份|Province|Text|50
城市|City|Text|50
头像|AvatarUrl|Text|250
职业|Metier|Text|50
状态|Status|Text|50|Y
积分|Integral|Number
内容数|ContentQty|Number
回复数|ReplyQty|Number",
            PageData = "{\"Type\":null,\"ShowPager\":true,\"PageSize\":null,\"Tools\":[\"Enable\",\"Disable\"],\"Actions\":null,\"Columns\":[{\"Id\":\"UserName\",\"Name\":\"账号\",\"IsViewLink\":true,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"OpenId\",\"Name\":\"微信ID\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"NickName\",\"Name\":\"昵称\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Sex\",\"Name\":\"性别\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Country\",\"Name\":\"国家\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Province\",\"Name\":\"省份\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"City\",\"Name\":\"城市\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Metier\",\"Name\":\"职业\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Status\",\"Name\":\"状态\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Integral\",\"Name\":\"积分\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"ContentQty\",\"Name\":\"内容数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"ReplyQty\",\"Name\":\"回复数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null}]}",
            FormData = "{\"Width\":800,\"Maximizable\":false,\"DefaultMaximized\":false,\"IsContinue\":false,\"LabelSpan\":null,\"WrapperSpan\":null,\"NoFooter\":false,\"Fields\":[{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"UserName\",\"Name\":\"账号\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"OpenId\",\"Name\":\"微信ID\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":2,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"NickName\",\"Name\":\"昵称\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":2,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Sex\",\"Name\":\"性别\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":3,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Country\",\"Name\":\"国家\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":3,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Province\",\"Name\":\"省份\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":4,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"City\",\"Name\":\"城市\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":4,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Metier\",\"Name\":\"职业\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":5,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Status\",\"Name\":\"状态\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":5,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Integral\",\"Name\":\"积分\",\"Type\":3,\"Length\":null,\"Required\":false},{\"Row\":6,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"ContentQty\",\"Name\":\"内容数\",\"Type\":3,\"Length\":null,\"Required\":false},{\"Row\":6,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"ReplyQty\",\"Name\":\"回复数\",\"Type\":3,\"Length\":null,\"Required\":false}]}"
        };
    }
    #endregion

    #region 文档管理
    private static SysModule GetDmUpdateLogList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "DmUpdateLogList",
            Name = "更新日志",
            Icon = "history",
            Description = "查询和维护更新日志。",
            Target = ModuleType.Custom.ToString(),
            Url = "/dms/updatelogs",
            Sort = 1,
            Enabled = true,
            EntityData = "",
            PageData = "",
            FormData = ""
        };
    }

    private static SysModule GetDmDocumentList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "DmDocumentList",
            Name = "项目文档",
            Icon = "file-word",
            Description = "查询和维护项目文档。",
            Target = ModuleType.Custom.ToString(),
            Url = "/dms/documents",
            Sort = 2,
            Enabled = true,
            EntityData = "",
            PageData = "",
            FormData = ""
        };
    }

    private static SysModule GetDmArticleList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "DmArticleList",
            Name = "文章管理",
            Icon = "file-word",
            Description = "查询和维护文章信息。",
            Target = ModuleType.Custom.ToString(),
            Url = "/dms/articles",
            Sort = 3,
            Enabled = true,
            EntityData = "CmPost",
            PageData = "{\"Type\":null,\"ShowPager\":true,\"PageSize\":null,\"ToolSize\":null,\"ActionSize\":null,\"Tools\":[\"New\",\"DeleteM\"],\"Actions\":[\"Edit\",\"Delete\"],\"Columns\":[{\"Id\":\"CategoryId\",\"Name\":\"分类ID\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Title\",\"Name\":\"标题\",\"IsViewLink\":true,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Content\",\"Name\":\"内容\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Status\",\"Name\":\"状态\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"PublishTime\",\"Name\":\"发布时间\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"ViewQty\",\"Name\":\"浏览数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"LikeQty\",\"Name\":\"喜欢数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null}]}",
            FormData = "{\"Width\":1000,\"Maximizable\":false,\"DefaultMaximized\":false,\"IsContinue\":false,\"NoFooter\":false,\"Fields\":[{\"Row\":1,\"Column\":1,\"Span\":null,\"CustomField\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"CategoryId\",\"Name\":\"分类ID\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":1,\"Column\":1,\"Span\":null,\"CustomField\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Title\",\"Name\":\"标题\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":1,\"Column\":1,\"Span\":null,\"CustomField\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Content\",\"Name\":\"内容\",\"Type\":1,\"Length\":null,\"Required\":true}]}"
        };
    }
    #endregion

    #region 交流管理
    private static SysModule GetImCategoryList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "ImCategoryList",
            Name = "交流分类",
            Icon = "ordered-list",
            Description = "查询和维护交流板块分类列表。",
            Target = ModuleType.Custom.ToString(),
            Url = "/ims/categories",
            Sort = 1,
            Enabled = true,
            EntityData = @"分类信息|CmCategory
分类类型|Type|Text|50|Y
上级分类|ParentId|Text|50|Y
代码|Code|Text|50|Y
名称|Name|Text|50|Y
图标|Icon|Text|50
顺序|Sort|Number||Y
启用|Enabled|Switch
备注|Note|TextArea",
            PageData = "{\"Type\":null,\"ShowPager\":false,\"PageSize\":null,\"Tools\":[\"New\",\"DeleteM\"],\"Actions\":null,\"Columns\":[{\"Id\":\"Code\",\"Name\":\"代码\",\"IsViewLink\":true,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Name\",\"Name\":\"名称\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Icon\",\"Name\":\"图标\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Sort\",\"Name\":\"顺序\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Enabled\",\"Name\":\"启用\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Note\",\"Name\":\"备注\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null}]}",
            FormData = "{\"Width\":null,\"Maximizable\":false,\"DefaultMaximized\":false,\"IsContinue\":false,\"LabelSpan\":null,\"WrapperSpan\":null,\"NoFooter\":false,\"Fields\":[{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Code\",\"Name\":\"代码\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Name\",\"Name\":\"名称\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Icon\",\"Name\":\"图标\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Sort\",\"Name\":\"顺序\",\"Type\":3,\"Length\":null,\"Required\":true},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Enabled\",\"Name\":\"启用\",\"Type\":4,\"Length\":null,\"Required\":true},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Note\",\"Name\":\"备注\",\"Type\":1,\"Length\":null,\"Required\":false}]}"
        };
    }

    private static SysModule GetImPostList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "ImPostList",
            Name = "内容管理",
            Icon = "file-word",
            Description = "查询和维护网站用户讨论的内容信息。",
            Target = ModuleType.Custom.ToString(),
            Url = "/ims/posts",
            Sort = 2,
            Enabled = true,
            EntityData = @"内容信息|CmPost
内容类型|Type|Text|50|Y
分类ID|CategoryId|Text|50
标题|Title|Text|250|Y
内容|Content|TextArea||Y
摘要|Summary|Text|500
标签|Tags|Text|200
图片|Image|Text|250
附件|Files|Text|250
状态|Status|Text|50|Y
发布时间|PublishTime|Date
浏览数|ViewQty|Number
喜欢数|LikeQty|Number
排名|RankNo|Number",
            PageData = "{\"Type\":null,\"ShowPager\":true,\"PageSize\":null,\"Tools\":[\"DeleteM\"],\"Actions\":[\"Edit\",\"Delete\"],\"Columns\":[{\"Id\":\"Title\",\"Name\":\"标题\",\"IsViewLink\":true,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Summary\",\"Name\":\"摘要\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Tags\",\"Name\":\"标签\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Image\",\"Name\":\"图片\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Status\",\"Name\":\"状态\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"PublishTime\",\"Name\":\"发布时间\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"ViewQty\",\"Name\":\"浏览数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"LikeQty\",\"Name\":\"喜欢数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"RankNo\",\"Name\":\"排名\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null}]}",
            FormData = "{\"Width\":1000,\"Maximizable\":false,\"DefaultMaximized\":false,\"IsContinue\":false,\"LabelSpan\":null,\"WrapperSpan\":null,\"NoFooter\":false,\"Fields\":[{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"CategoryId\",\"Name\":\"分类ID\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":1,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Title\",\"Name\":\"标题\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":2,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Content\",\"Name\":\"内容\",\"Type\":1,\"Length\":null,\"Required\":true},{\"Row\":3,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Summary\",\"Name\":\"摘要\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":4,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Tags\",\"Name\":\"标签\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":4,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Image\",\"Name\":\"图片\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":5,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Files\",\"Name\":\"附件\",\"Type\":0,\"Length\":null,\"Required\":false},{\"Row\":6,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"Status\",\"Name\":\"状态\",\"Type\":0,\"Length\":null,\"Required\":true},{\"Row\":6,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"PublishTime\",\"Name\":\"发布时间\",\"Type\":2,\"Length\":null,\"Required\":false},{\"Row\":7,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"ViewQty\",\"Name\":\"浏览数\",\"Type\":3,\"Length\":null,\"Required\":false},{\"Row\":7,\"Column\":1,\"Span\":null,\"CategoryType\":null,\"Category\":null,\"Placeholder\":null,\"ReadOnly\":false,\"MultiFile\":false,\"Id\":\"LikeQty\",\"Name\":\"喜欢数\",\"Type\":3,\"Length\":null,\"Required\":false}]}"
        };
    }

    private static SysModule GetImReplyList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "ImReplyList",
            Name = "回复管理",
            Icon = "message",
            Description = "查询和维护用户回复内容。",
            Target = ModuleType.Custom.ToString(),
            Url = "/ims/replies",
            Sort = 3,
            Enabled = true,
            EntityData = @"回复信息|CmReply
业务类型|BizType|Text|50|Y
业务ID|BizId|Text|50|Y
用户ID|UserId|Text|50|Y
内容|Content|TextArea||Y
回复时间|ReplyTime|Date||Y
喜欢数|LikeQty|Number
回复数|ReplyQty|Number",
            PageData = "{\"Type\":null,\"ShowPager\":true,\"PageSize\":null,\"Tools\":[\"DeleteM\"],\"Actions\":[\"Delete\"],\"Columns\":[{\"Id\":\"BizType\",\"Name\":\"业务类型\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"BizId\",\"Name\":\"业务ID\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"UserId\",\"Name\":\"用户ID\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"Content\",\"Name\":\"内容\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"ReplyTime\",\"Name\":\"回复时间\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"LikeQty\",\"Name\":\"喜欢数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"ReplyQty\",\"Name\":\"回复数\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null}]}",
            FormData = ""
        };
    }

    private static SysModule GetImLogList(string parentId)
    {
        return new SysModule
        {
            ParentId = parentId,
            Code = "ImLogList",
            Name = "操作日志",
            Icon = "clock-circle",
            Description = "查询和维护用户操作日志。",
            Target = ModuleType.Custom.ToString(),
            Url = "/ims/logs",
            Sort = 4,
            Enabled = true,
            EntityData = @"用户操作日志|CmLog
业务类型|BizType|Text|50|Y
日志类型|LogType|Text|50|Y
业务ID|BizId|Text|50|Y
用户ID|UserId|Text|50|Y
用户IP|UserIP|Text|50",
            PageData = "{\"Type\":null,\"ShowPager\":true,\"PageSize\":null,\"Tools\":null,\"Actions\":null,\"Columns\":[{\"Id\":\"BizType\",\"Name\":\"业务类型\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"LogType\",\"Name\":\"日志类型\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"BizId\",\"Name\":\"业务ID\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"UserId\",\"Name\":\"用户ID\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"UserIP\",\"Name\":\"用户IP\",\"IsViewLink\":false,\"IsQuery\":false,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null},{\"Id\":\"CreateTime\",\"Name\":\"创建时间\",\"IsViewLink\":false,\"IsQuery\":true,\"IsQueryAll\":false,\"IsSum\":false,\"IsSort\":false,\"DefaultSort\":null,\"Fixed\":null,\"Width\":null,\"Align\":null}]}",
            FormData = ""
        };
    }
    #endregion
}