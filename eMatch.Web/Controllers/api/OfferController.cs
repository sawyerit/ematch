using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Services.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace eMatch.Web.Controllers.api
{
    [RoutePrefix("api")]
    public class OfferController : ApiController
    {

        private readonly IOfferService _offers;
        private readonly IUserService _user;
        //private ILogger _logger;

        public OfferController(IOfferService offers, IUserService user)
        {
            _offers = offers;
            _user = user;
        }

        [Route("offers")]
        [HttpPost]
        public HttpResponseMessage CreateOffer([FromBody] Offer offer)
        {
            try
            {
                var user = _user.GetUser("needtochangethis");
                return Request.CreateResponse(HttpStatusCode.Created, _offers.CreateOffer(offer));
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, message);
            }
        }

        [Route("offers")]
        [HttpGet]
        public HttpResponseMessage GetAllOffers()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, _offers.GetAllOffers());
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, message);
            }
        }
    }     
}
