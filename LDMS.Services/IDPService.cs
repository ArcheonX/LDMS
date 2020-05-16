using Dapper;
using LDMS.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using LDMS.Daos;
using LDMS.ViewModels;

namespace LDMS.Services
{
    public class IDPService : ILDMSService
    {
        private readonly ILogger<IDPService> _logger;
        public IDPService(
           ILogger<IDPService> logger,
           ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _logger = logger;
        }

        public ViewModels.LDMS_T_IDP_SubTopic UpdateIDPSubtopicStatus(string ID_IDP_SubTopic, string SubTopicStatus)
        {

            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                p.Add("@ID_IDP_SubTopic", ID_IDP_SubTopic);
               
                p.Add("@SubTopicStatus", SubTopicStatus);
                p.Add("@UpdateBy", CurrentUserId);

                ViewModels.LDMS_T_IDP_SubTopic ret = conn.Query<ViewModels.LDMS_T_IDP_SubTopic>(_schema + ".[sp_T_IDP_SubTopic_UpdateStatus]", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                return ret;
            }
        }

        public ViewModels.LDMS_T_IDP GetMyIDP()
        {
            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();
                    //if (ID_Employee != null) p.Add("@ID_Employee", ID_Employee);
                    
                    p.Add("@ID_Employee", CurrentUserId); //CurrentUserId

                    var myIDP = conn.Query<ViewModels.LDMS_T_IDP>(_schema + ".[sp_T_IDP_Select_Employee_ID]", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    return myIDP;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public ViewModels.Paging_Result GetTopics( string ID_IDP, string Year, Paging_Search criteria)
        {
            ViewModels.Paging_Result ret = new ViewModels.Paging_Result();
            List<ViewModels.LDMS_T_Topic_Result> TopicList = new List<ViewModels.LDMS_T_Topic_Result>();
            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();
                    //if (ID_Employee != null) p.Add("@ID_Employee", ID_Employee);

                    p.Add("@ID_IDP", ID_IDP); //CurrentUserId

                    if (Year != null || Year != "")
                        p.Add("@IDP_Year", Year); 
                    
                    
                    p.Add("@PageNum", criteria.PageNum);
                    p.Add("@PageSize", criteria.PageSize);
                    p.Add("@SortField", criteria.SortField);
                    p.Add("@SortOrder", criteria.SortOrder);

                    var grid = conn.QueryMultiple("[dbo].[sp_T_IDP_Topic_Select_IDP_ID]", p, commandType: CommandType.StoredProcedure);

                    TopicList = grid.Read<ViewModels.LDMS_T_Topic_Result>().ToList();
                    var totalRec = grid.Read().ToList();

                    ret.data = TopicList;
                    ret.recordsTotal = totalRec[0].TotalRecords;
                    ret.recordsFiltered = totalRec[0].TotalRecords;
                    ret.draw = 0;

                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public List<ViewModels.LDMS_T_IDP_Topic> GetTopicsList(string ID_IDP, string Year)
        {
            
            List<ViewModels.LDMS_T_IDP_Topic> TopicList = new List<ViewModels.LDMS_T_IDP_Topic>();
            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();
                    
                    p.Add("@ID_IDP", ID_IDP); //CurrentUserId

                    if (Year != null || Year != "")
                        p.Add("@IDP_Year", Year);

                    var grid = conn.QueryMultiple("[dbo].[sp_T_IDP_Topic_Select_IDP_ID_list]", p, commandType: CommandType.StoredProcedure);

                    TopicList = grid.Read<ViewModels.LDMS_T_IDP_Topic>().ToList();

                    return TopicList;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public List<ViewModels.LDMS_T_IDP_SubTopic> GetSubTopicsList(string ID_IDP_Topic)
        {

            List<ViewModels.LDMS_T_IDP_SubTopic> SubTopicList = new List<ViewModels.LDMS_T_IDP_SubTopic>();
            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();

                    p.Add("@ID_IDP_Topic", ID_IDP_Topic); //CurrentUserId

                    var grid = conn.QueryMultiple("[dbo].[sp_T_SUB_Topic_Select_TOPIC_ID]", p, commandType: CommandType.StoredProcedure);

                    SubTopicList = grid.Read<ViewModels.LDMS_T_IDP_SubTopic>().ToList();

                    return SubTopicList;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public ViewModels.LDMS_T_IDP_SubTopic GetSubTopicsID(string ID_Sub_Topic_ID)
        {

            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();

                    p.Add("@ID_SUB_Topic_ID", ID_Sub_Topic_ID); 

                    var ret  = conn.Query<ViewModels.LDMS_T_IDP_SubTopic>(_schema + ".[sp_T_SUB_Topic_Select_SUB_TOPIC_ID]", p, commandType: CommandType.StoredProcedure).FirstOrDefault();



                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public int CreateIDPHeadReview(string ID_IDP_SubTopic, string EmployeeReport, string AttachFilePath, string AttachFileName)
        {

            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();

                    p.Add("@ID_IDP_SubTopic", ID_IDP_SubTopic);
                    p.Add("@EmployeeReport", EmployeeReport);
                    p.Add("@AttachFilePath", AttachFilePath);
                    p.Add("@AttachFileName", AttachFileName);
                    p.Add("@ID_PostByEmployee", CurrentUserId);
                    p.Add("@ReviewStatus", 1);

                    int ret = conn.Query<int>(_schema + ".[sp_T_IDP_ReviewHead_Insert]", p, commandType: CommandType.StoredProcedure).Single();


                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public ViewModels.LDMS_T_IDP_ReviewHead GetIDPReviewHeadCurrent(string ID_IDP_SubTopic)
        {
            using (IDbConnection conn = Connection)
            {
                
                try
                {

                    var p = new DynamicParameters();
                    if (ID_IDP_SubTopic != null) p.Add("@ID_IDP_SubTopic", ID_IDP_SubTopic);

                    var idpReviewHead = conn.Query<ViewModels.LDMS_T_IDP_ReviewHead>(_schema + ".[sp_T_IDP_ReviewHead_CurrentSelect]", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    return idpReviewHead;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public List<ViewModels.LDMS_T_IDP_ReviewHead> GetIDPReviewHead(string ID_IDP_SubTopic)
        {
            using (IDbConnection conn = Connection)
            {
                List<ViewModels.LDMS_T_IDP_ReviewHead> idpReviewHead = new List<ViewModels.LDMS_T_IDP_ReviewHead>();
                try
                {

                    var p = new DynamicParameters();
                    if (ID_IDP_SubTopic != null) p.Add("@ID_IDP_SubTopic", ID_IDP_SubTopic);

                    idpReviewHead = conn.Query<ViewModels.LDMS_T_IDP_ReviewHead>(_schema + ".[sp_T_IDP_ReviewHead_Select]", p, commandType: CommandType.StoredProcedure).ToList();

                    return idpReviewHead;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public int CreateIDPDetailReview(string ID_IDP_ReviewHead, string PostDetail, string AttachFilePath, string AttachFileName)
        {

            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();

                    p.Add("@ID_IDP_ReviewHead", ID_IDP_ReviewHead);
                    p.Add("@PostDetail", PostDetail);
                    p.Add("@AttachFilePath", AttachFilePath);
                    p.Add("@AttachFileName", AttachFileName);
                    p.Add("@PostBy_EmployeeID", CurrentUserId);

                    int ret = conn.Query<int>(_schema + ".[sp_T_IDP_ReviewDetail_Insert]", p, commandType: CommandType.StoredProcedure).Single();


                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public List<ViewModels.LDMS_T_IDP_ReviewDetail> GetIDPReviewDetail(string ID_IDP_ReviewHead)
        {
            using (IDbConnection conn = Connection)
            {
                List<ViewModels.LDMS_T_IDP_ReviewDetail> idpReviewDetail = new List<ViewModels.LDMS_T_IDP_ReviewDetail>();
                try
                {

                    var p = new DynamicParameters();
                    if (ID_IDP_ReviewHead != null) p.Add("@ID_IDP_ReviewHead", ID_IDP_ReviewHead);

                    idpReviewDetail = conn.Query<ViewModels.LDMS_T_IDP_ReviewDetail>(_schema + ".[sp_T_IDP_ReviewDetail_Select]", p, commandType: CommandType.StoredProcedure).ToList();

                    return idpReviewDetail;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}
