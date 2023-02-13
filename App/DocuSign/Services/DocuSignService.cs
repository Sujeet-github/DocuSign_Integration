using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static DocuSign.eSign.Api.EnvelopesApi;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;

using DocuSign.Exceptions;
using DocuSign.Models;
using System.Text;
using DocuSign.eSign.Client.Auth;

namespace DocuSign.Services
{
    public class DocuSignService : IDocuSignService
    {
        private string _accountId;

        private ApiClient _apiClient;

        private readonly DocuSignConfiguration _config;

        public DocuSignService(
            DocuSignConfiguration config
        )
        {
            _config = config;
        }

        public EnvelopeSummary CreateEnvelope(SignerInfo signerInfo, Base64Document documentToSign)
        {
            if (signerInfo == null)
            {
                throw new ArgumentNullException(nameof(signerInfo));
            }

            if (documentToSign == null)
            {
                throw new ArgumentNullException(nameof(documentToSign));
            }

            this.InitApiClient();

            try
            {
                EnvelopeDefinition envelopeDefinition = this.GetEnvelopeDefinition(signerInfo, documentToSign);

                var envelopesApi = new EnvelopesApi(_apiClient.Configuration);
                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(_accountId, envelopeDefinition);

                return envelopeSummary;
            }
            catch (Exception ex)
            {
                throw new DocuSignServiceException("Failed to create envelope. See inner exception for details.", ex);
            }
        }

        public ViewUrl CreateSigningUrl(string envelopeId, SignerInfo signerInfo, string signingResultReturnUrl)
        {
            if (envelopeId == null)
            {
                throw new ArgumentNullException(nameof(envelopeId));
            }

            if (signerInfo == null)
            {
                throw new ArgumentNullException(nameof(signerInfo));
            }

            if (signingResultReturnUrl == null)
            {
                throw new ArgumentNullException(nameof(signingResultReturnUrl));
            }

            var recipientViewRequest = new RecipientViewRequest
            {
                AuthenticationMethod = "email",
                ClientUserId = signerInfo.Id,
                Email = signerInfo.Email,
                ReturnUrl = $"{signingResultReturnUrl}?envelopeId={envelopeId}",
                UserName = signerInfo.Name
            };

            this.InitApiClient();

            try
            {
                var envelopesApi = new EnvelopesApi();
                ViewUrl recipientView = envelopesApi.CreateRecipientView(_accountId, envelopeId, recipientViewRequest);

                return recipientView;
            }
            catch (Exception ex)
            {
                throw new DocuSignServiceException("Failed to create signing URL. See inner exception for details.", ex);
            }
        }

        public Envelope GetEnvelope(string envelopeId)
        {
            if (envelopeId == null)
            {
                throw new ArgumentNullException(nameof(envelopeId));
            }

            this.InitApiClient();

            try
            {
                var envelopesApi = new EnvelopesApi(_apiClient.Configuration);
                Envelope envelope = envelopesApi.GetEnvelope(_accountId, envelopeId);

                return envelope;
            }
            catch (Exception ex)
            {
                throw new DocuSignServiceException("Failed to get envelope. See inner exception for details.", ex);
            }
        }

        public IEnumerable<Envelope> GetEnvelopesWithUpdatedStatus(DateTime fromDate)
        {
            var envelopes = new List<Envelope>();

            var options = new ListStatusChangesOptions
            {
                fromDate = fromDate.ToString("o"),
                status = "any",
            };

            this.InitApiClient();

            try
            {
                var envelopesApi = new EnvelopesApi(_apiClient.Configuration);

                EnvelopesInformation envelopesInformation = null;
                do
                {
                    envelopesInformation = envelopesApi.ListStatusChanges(_accountId, options);
                    envelopes.AddRange(envelopesInformation.Envelopes);

                    options.startPosition = envelopes.Count.ToString();
                }
                while (!string.IsNullOrEmpty(envelopesInformation.NextUri));

                return envelopes;
            }
            catch (Exception ex)
            {
                throw new DocuSignServiceException("Failed to get envelopes with updated status. See inner exception for details.", ex);
            }
        }

        public Base64Document GetSignedDocumentFromEnvelope(string envelopeId)
        {
            if (envelopeId == null)
            {
                throw new ArgumentNullException(nameof(envelopeId));
            }

            this.InitApiClient();

            try
            {
                var envelopesApi = new EnvelopesApi(_apiClient.Configuration);

                EnvelopeDocumentsResult documentsResult = envelopesApi.ListDocuments(_accountId, envelopeId);

                EnvelopeDocument envelopeDocument = documentsResult.EnvelopeDocuments.First(d => d.Type == "content");

                using (MemoryStream memoryStream = (MemoryStream)envelopesApi.GetDocument(_accountId, documentsResult.EnvelopeId, envelopeDocument.DocumentId))
                {
                    var signedDocument = new Base64Document
                    {
                        Content = Convert.ToBase64String(memoryStream.ToArray()),
                        Name = envelopeDocument.Name,
                    };
                    return signedDocument;
                }
            }
            catch (Exception ex)
            {
                throw new DocuSignServiceException("Failed to get signed document from envelope. See inner exception for details.", ex);
            }
        }


        private EnvelopeDefinition GetEnvelopeDefinition(SignerInfo signerInfo, Base64Document documentToSign)
        {
            var envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = _config.EmailSubject,
                Documents = new List<Document>
                {
                    new Document
                    {
                        DocumentId = "1",
                        DocumentBase64 = documentToSign.Content,
                        Name = documentToSign.Name,
                        FileExtension = System.IO.Path.GetExtension(documentToSign.Name)
                    }
                },
                Recipients = new Recipients
                {
                    Signers = new List<Signer>
                    {
                        new Signer
                        {
                            ClientUserId = signerInfo.Id,
                            Email = signerInfo.Email,
                            Name = signerInfo.Name,
                            RecipientId = "1",
                            RoutingOrder = "1",
                            Tabs = new Tabs
                            {
                                SignHereTabs = new List<SignHere>
                                {
                                    new SignHere
                                    {
                                        AnchorString = _config.SignHereTagAnchorString,
                                        AnchorXOffset = "1",
                                        AnchorYOffset = "0",
                                        AnchorIgnoreIfNotPresent = "false",
                                        AnchorUnits = "inches",
                                        DocumentId = "1",
                                        PageNumber = "1",
                                        RecipientId = "1"
                                    }
                                }
                            }
                        }
                    }
                },
                Status = "sent"
            };

            if (!string.IsNullOrEmpty(_config.SignDateTagAnchorString))
            {
                envelopeDefinition.Recipients.Signers.First().Tabs.DateSignedTabs = new List<DateSigned>
                {
                        new DateSigned
                        {
                            AnchorString = _config.SignDateTagAnchorString,
                            AnchorXOffset = "1",
                            AnchorYOffset = "0",
                            AnchorIgnoreIfNotPresent = "true",
                            AnchorUnits = "inches",
                            DocumentId = "1",
                            PageNumber = "1",
                            RecipientId = "1"
                        }
                };
            }


            return envelopeDefinition;
        }

        private void InitApiClient()
        {
            if (_apiClient != null)
            {
                return;
            }

            try
            {                              
                string authServer = string.Empty;
                if (!string.IsNullOrEmpty(_config.RestApiUrl))
                {
                    if (_config.RestApiUrl.ToLower().Contains("demo"))
                    {
                        authServer = "account-d.docusign.com";
                    }
                    else
                    {
                        authServer = "account.docusign.com";
                    }
                }
                var scopes = new List<string> { "signature" , "impersonation" };

                _apiClient = new ApiClient(_config.RestApiUrl);

                OAuth.OAuthToken authToken = _apiClient.RequestJWTUserToken(_config.IntegratorKey, _config.UserId, authServer, Encoding.UTF8.GetBytes(_config.PrivateKey), 1, scopes);

                string accessToken = authToken.access_token;

                OAuth.UserInfo userInfo = _apiClient.GetUserInfo(accessToken);

                if(userInfo != null && userInfo.Accounts != null)
                {
                    foreach (var item in userInfo.Accounts)
                    {
                        if (item.IsDefault == "true")
                        {
                            _accountId = item.AccountId;
                            _apiClient.SetBasePath(item.BaseUri + "/restapi");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DocuSignServiceException("Failed to initialize DocuSign service. See inner exception for details.", ex);
            }
        }
    }
}
