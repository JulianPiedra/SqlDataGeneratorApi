﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SqlDataGenerator.Abstract.DependencyInjection;
using SqlDataGenerator.Logic.GenerationUtils;
using SqlDataGenerator.Models;
using SQLDataGeneratorAPI.DataAccess.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataGenerator.Logic.GenerationLogic
{
    public class EmailGeneration : IEmailGeneration
    {
        private readonly SQLGeneratorContext Context;
        public EmailGeneration(SQLGeneratorContext context)
        {
            Context = context;
        }
        public async Task<BusinessLogicResponse> GenerateEmail(Record records)
        {

            try
            {
                // If the record name is not provided, use the default name "gender"
                var key = string.IsNullOrEmpty(records.RecordName) ? "gender" : records.RecordName;


                var random = new Random();

                //Fetch the data as a string list to later merge them as a single string
                var randomNames = await FetchFromDatabase.FetchStringListFromDatabase(
                            records.Records,
                            Context.FirstName,
                            f => f.FirstName1);

                var randomLastNames = await FetchFromDatabase.FetchStringListFromDatabase(
                            records.Records,
                            Context.LastName,
                            f => f.LastName1);

                var randomEmailExtension = await FetchFromDatabase.FetchStringListFromDatabase(
                            records.Records,
                            Context.Email,
                            f => f.EmailExtension);

                // Merge the results of all tasks
                var result = randomNames
                .Zip(randomLastNames, (firstName, lastName) => new { FirstName = firstName.ToLower(), LastName = lastName.ToLower() })
                .Zip(randomEmailExtension, (combined, emailExtension) => new Dictionary<string, object>
                {
                    { key, $"{combined.FirstName}{combined.LastName}{RandomDataGeneration.GenerateRandomNumber(999, random)}{emailExtension}" }
                }).ToList();


                return new BusinessLogicResponse { StatusCode = 200, ObjectResponse = result };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse { StatusCode = 500, Message = ex.Message };
            }
        }
    }
}
