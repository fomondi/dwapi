﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dwapi.ExtractsManagement.Core.Interfaces.Repository;
using Dwapi.ExtractsManagement.Core.Model;
using Dwapi.SharedKernel.Enum;
using Dwapi.SharedKernel.Infrastructure.Repository;

namespace Dwapi.ExtractsManagement.Infrastructure.Repository
{
    public class ExtractHistoryRepository :BaseRepository<ExtractHistory,Guid>, IExtractHistoryRepository
    {
        public ExtractHistoryRepository(ExtractsContext context) : base(context)
        {
        }

        public  void ClearHistory(Guid extractId)
        {
            Task.Run(() =>ClearHistory(new List<Guid> {extractId}));
        }

        public async Task<int> ClearHistory(List<Guid> extractIds)
        {
            var count = 0;
            var histories = DbSet
                .Where(x => extractIds.Contains(x.ExtractId)).ToList();
            if (histories.Any())
            {
                DbSet.RemoveRange(histories);
                count= await SaveChangesAsync();
            }

            return count;
        }

        public ExtractHistory GetLatest(Guid extractId)
        {
            return DbSet
                .Where(x => x.ExtractId == extractId)
                .OrderByDescending(x => x.StatusDate)
                .FirstOrDefault();
        }

        public ExtractHistory GetLatest(Guid extractId, ExtractStatus status, ExtractStatus otherStatus)
        {
            return DbSet
                .Where(x => x.ExtractId == extractId && (x.Status == status || x.Status == otherStatus))
                .OrderByDescending(x => x.StatusDate)
                .FirstOrDefault();
        }

        public IEnumerable<ExtractHistory> GetAllExtractStatus(Guid extractId)
        {
            return GetAll().Where(x => x.ExtractId == extractId);
        }

        public void UpdateStatus(Guid extractId, ExtractStatus status,int? stats, string statusInfo = "", bool express = false)
        {
            if(express)
            {
                var history = new ExtractHistory(status, stats, statusInfo, extractId);
                Create(history);
                SaveChanges();
                return;
            }

            var started= DbSet.Any(x => x.ExtractId == extractId&&x.Status==status);

            if (!started)
            {
                var history=new ExtractHistory(status, stats,statusInfo,extractId);
                Create(history);
                SaveChanges();
            }
        }

        public void DwhUpdateStatus(Guid extractId, ExtractStatus status, int? stats, string statusInfo = "")
        {
                var history = new ExtractHistory(status, stats, statusInfo, extractId);
                Create(history);
                SaveChanges();
        }



        public void Complete(Guid extractId)
        {
            var history = new ExtractHistory(ExtractStatus.Idle,extractId);
            Create(history);
            SaveChanges();
        }
    }
}