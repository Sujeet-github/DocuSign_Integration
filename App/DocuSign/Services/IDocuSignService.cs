using System;
using System.Collections.Generic;
using DocuSign.eSign.Model;
using DocuSign.Models;

namespace DocuSign.Services
{
    public interface IDocuSignService
    {
        /// <summary>
        /// Creates the envelope for signature.
        /// </summary>
        /// <param name="signerInfo">The information about signer.</param>
        /// <param name="documentToSign">The document to sign.</param>
        /// <returns>Summary information about created envelope.</returns>
        /// <exception cref="Exceptions.DocuSignServiceException">Thrown when error occurred.</exception>
        EnvelopeSummary CreateEnvelope(SignerInfo signerInfo, Base64Document documentToSign);

        /// <summary>
        /// Creates URL for signing the envelope.
        /// </summary>
        /// <param name="envelopeId">The envelope identifier.</param>
        /// <param name="signerInfo">The signer information.</param>
        /// <param name="signingResultReturnUrl">The URL that user will be redirected to after signing.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.DocuSignServiceException">Thrown when error occurred.</exception>
        ViewUrl CreateSigningUrl(string envelopeId, SignerInfo signerInfo, string signingResultReturnUrl);

        /// <summary>
        /// Gets the detailed information about envelope.
        /// </summary>
        /// <param name="envelopeId">The envelope identifier.</param>
        /// <returns>Envelope.</returns>
        /// <exception cref="Exceptions.DocuSignServiceException">Thrown when error occurred.</exception>
        Envelope GetEnvelope(string envelopeId);

        /// <summary>
        /// Gets the envelopes with updated status.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <returns>Collection of envelopes, that have changed their status after <paramref name="fromDate"/> date.</returns>
        /// <exception cref="Exceptions.DocuSignServiceException">Thrown when error occurred.</exception>
        IEnumerable<Envelope> GetEnvelopesWithUpdatedStatus(DateTime fromDate);

        /// <summary>
        /// Gets the signed document from envelope.
        /// </summary>
        /// <param name="envelopeId">The envelope identifier.</param>
        /// <returns>Signed document.</returns>
        /// <exception cref="Exceptions.DocuSignServiceException">Thrown when error occurred.</exception>
        Base64Document GetSignedDocumentFromEnvelope(string envelopeId);
    }
}
