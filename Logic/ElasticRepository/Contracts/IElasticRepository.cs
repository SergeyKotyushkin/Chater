﻿using Nest;

namespace Logic.ElasticRepository.Contracts
{
    public interface IElasticRepository
    {
        bool CheckConnection();

        ElasticClient GetElasticClient();
    }
}