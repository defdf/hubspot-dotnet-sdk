﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HubSpot.Model;
using HubSpot.Model.Contacts;
using HubSpot.Model.Contacts.Properties;
using Kralizek.Extensions.Http;

namespace HubSpot
{
    public partial class HttpHubSpotClient : IHubSpotContactClient
    {
        IHubSpotContactPropertyClient IHubSpotContactClient.Properties => this;

        IHubSpotContactPropertyGroupClient IHubSpotContactClient.PropertyGroups => this;

        async Task<Contact> IHubSpotContactClient.GetByIdAsync(long contactId, IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships)
        {
            var builder = new HttpQueryStringBuilder();

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);

            try
            {
                var contact = await _client.GetAsync<Contact>($"/contacts/v1/contact/vid/{contactId}/profile", builder.BuildQuery());
                return contact;
            }
            catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException("Contact not found", ex);
            }
        }

        async Task<Contact> IHubSpotContactClient.GetByEmailAsync(string email, IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            var builder = new HttpQueryStringBuilder();

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);

            try
            {
                var contact = await _client.GetAsync<Contact>($"/contacts/v1/contact/email/{email}/profile", builder.BuildQuery());
                return contact;
            }
            catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException("Contact not found", ex);
            }
        }

        async Task<Contact> IHubSpotContactClient.GetByUserTokenAsync(string userToken, IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships)
        {
            if (string.IsNullOrEmpty(userToken))
            {
                throw new ArgumentNullException(nameof(userToken));
            }

            var builder = new HttpQueryStringBuilder();

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);

            try
            {
                var contact = await _client.GetAsync<Contact>($"/contacts/v1/contact/utk/{userToken}/profile", builder.BuildQuery());
                return contact;
            }
            catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException("Contact not found", ex);
            }
        }

        async Task<IReadOnlyDictionary<long, Contact>> IHubSpotContactClient.GetManyByIdAsync(IReadOnlyList<long> contactIds, IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships, bool includeDeletes)
        {
            if (contactIds == null || contactIds.Count == 0)
            {
                return new Dictionary<long, Contact>();
            }

            if (contactIds.Count >= 100)
            {
                throw new ArgumentOutOfRangeException(nameof(contactIds), "Up to 100 contacts can be requested at the same time");
            }

            var builder = new HttpQueryStringBuilder();

            foreach (var id in contactIds)
            {
                builder.Add("vid", id.ToString());
            }

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);

            var contacts = await _client.GetAsync<Dictionary<long, Contact>>("/contacts/v1/contact/vids/batch/", builder.BuildQuery());

            return contacts;
        }

        async Task<IReadOnlyDictionary<long, Contact>> IHubSpotContactClient.GetManyByEmailAsync(IReadOnlyList<string> emails, IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships, bool includeDeletes)
        {
            if (emails == null || emails.Count == 0)
            {
                return new Dictionary<long, Contact>();
            }

            if (emails.Count >= 100)
            {
                throw new ArgumentOutOfRangeException(nameof(emails), "Up to 100 contacts can be requested at the same time");
            }

            var builder = new HttpQueryStringBuilder();

            foreach (var email in emails)
            {
                builder.Add("email", email);
            }

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);

            var contacts = await _client.GetAsync<Dictionary<long, Contact>>("/contacts/v1/contact/emails/batch/", builder.BuildQuery());

            return contacts;
        }

        async Task<ContactList> IHubSpotContactClient.GetAllAsync(IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships, int count, long? contactOffset)
        {
            if (count > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Up to 100 contacts can be requested at the same time");
            }

            var builder = new HttpQueryStringBuilder();

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);
            builder.Add("count", count.ToString());

            if (contactOffset.HasValue)
            {
                builder.Add("vidOffset", contactOffset.Value.ToString());
            }

            var list = await _client.GetAsync<ContactList>("/contacts/v1/lists/all/contacts/all", builder.BuildQuery());

            return list;
        }

        async Task<ContactList> IHubSpotContactClient.GetRecentlyUpdatedAsync(IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships, int count, long? contactOffset, DateTimeOffset? timeOffset)
        {
            if (count > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Up to 100 contacts can be requested at the same time");
            }

            var builder = new HttpQueryStringBuilder();

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);
            builder.Add("count", count.ToString());

            if (contactOffset.HasValue)
            {
                builder.Add("vidOffset", contactOffset.Value.ToString());
            }

            if (timeOffset.HasValue)
            {
                builder.Add("timeOffset", timeOffset.Value.ToUnixTimeMilliseconds().ToString());
            }

            var list = await _client.GetAsync<ContactList>("/contacts/v1/lists/recently_updated/contacts/recent", builder.BuildQuery());

            return list;
        }

        async Task<ContactList> IHubSpotContactClient.GetRecentlyCreatedAsync(IReadOnlyList<IProperty> properties, PropertyMode propertyMode, FormSubmissionMode formSubmissionMode, bool showListMemberships, int count, long? contactOffset, DateTimeOffset? timeOffset)
        {
            if (count > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Up to 100 contacts can be requested at the same time");
            }

            var builder = new HttpQueryStringBuilder();

            builder.AddProperties(properties);
            builder.AddPropertyMode(propertyMode);
            builder.AddFormSubmissionMode(formSubmissionMode);
            builder.AddShowListMemberships(showListMemberships);
            builder.Add("count", count.ToString());

            if (contactOffset.HasValue)
            {
                builder.Add("vidOffset", contactOffset.Value.ToString());
            }

            if (timeOffset.HasValue)
            {
                builder.Add("timeOffset", timeOffset.Value.ToUnixTimeMilliseconds().ToString());
            }

            var list = await _client.GetAsync<ContactList>("/contacts/v1/lists/all/contacts/recent", builder.BuildQuery());

            return list;
        }

        async Task<DeleteContactResponse> IHubSpotContactClient.DeleteAsync(long contactId)
        {
            var response = await _client.DeleteAsync<DeleteContactResponse>($"/contacts/v1/contact/vid/{contactId}");
            return response;
        }

        async Task<Contact> IHubSpotContactClient.CreateAsync(IReadOnlyList<ValuedProperty> properties)
        {
            var propertyList = new PropertyList<ValuedProperty>
            {
                Properties = properties
            };

            var contact = await _client.PostAsync<PropertyList<ValuedProperty>, Contact>("/contacts/v1/contact", propertyList);

            return contact;
        }

        async Task IHubSpotContactClient.UpdateByIdAsync(long contactId, IReadOnlyList<ValuedProperty> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return;
            }

            var propertyList = new PropertyList<ValuedProperty>
            {
                Properties = properties
            };

            await _client.PostAsync<PropertyList<ValuedProperty>>($"/contacts/v1/contact/vid/{contactId}/profile", propertyList);
        }

        async Task IHubSpotContactClient.UpdateByEmailAsync(string email, IReadOnlyList<ValuedProperty> properties)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            var propertyList = new PropertyList<ValuedProperty>
            {
                Properties = properties
            };

            await _client.PostAsync<PropertyList<ValuedProperty>>($"/contacts/v1/contact/email/{email}/profile", propertyList);
        }

        async Task<CreateOrUpdateResponse> IHubSpotContactClient.CreateOrUpdateByEmailAsync(string email, IReadOnlyList<ValuedProperty> properties)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            var propertyList = new PropertyList<ValuedProperty>
            {
                Properties = properties
            };

            var response = await _client.PostAsync<PropertyList<ValuedProperty>, CreateOrUpdateResponse>($"/contacts/v1/contact/createOrUpdate/email/{email}", propertyList);

            return response;
        }

        async Task<SearchResponse> IHubSpotContactClient.SearchAsync(string query, IReadOnlyList<IProperty> properties, int count, long? contactOffset)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (count > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Up to 100 contacts can be requested at the same time");
            }

            var builder = new HttpQueryStringBuilder();
            builder.Add("q", query);
            builder.AddProperties(properties);
            builder.Add("count", count.ToString());

            if (contactOffset.HasValue)
            {
                builder.Add("offset", contactOffset.Value.ToString());
            }

            var response = await _client.GetAsync<SearchResponse>("/contacts/v1/search/query", builder.BuildQuery());

            return response;
        }

        async Task IHubSpotContactClient.MergeAsync(long primaryContactId, long secondaryContactId)
        {
            var payload = new
            {
                vidToMerge = secondaryContactId
            };

            await _client.PostAsync<object>($"/contacts/v1/contact/merge-vids/{primaryContactId}/", payload);
        }
    }
}