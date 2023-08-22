﻿namespace Store.Web.Contractors
{
    public interface IWebContractorService
    {
        string Name { get; }  
        Uri StartSession(IReadOnlyDictionary<string, string> parameters, Uri returnUri);
        Task<Uri> StartSessionAsync(IReadOnlyDictionary<string, string> parameters, Uri returnUri);
    }
}