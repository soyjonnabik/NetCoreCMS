﻿using System;
using System.Collections.Generic;
using System.Linq;
using NetCoreCMS.Framework.Core.Mvc.Models;
using NetCoreCMS.Framework.Core.Mvc.Services;
using NetCoreCMS.Modules.News.Repository;
using NetCoreCMS.Modules.News.Models;
using Microsoft.EntityFrameworkCore;

namespace NetCoreCMS.Modules.News.Services
{
    public class NeCategoryService : IBaseService<NeCategory>
    {
        private readonly NeCategoryRepository _entityRepository;

        public NeCategoryService(NeCategoryRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }

        public NeCategory Get(long entityId, bool isAsNoTracking = false)
        {
            return _entityRepository.Get(entityId, isAsNoTracking);
        }

        public NeCategory Save(NeCategory entity)
        {
            _entityRepository.Add(entity);
            _entityRepository.SaveChange();
            return entity;
        }

        public NeCategory Update(NeCategory entity)
        {
            var oldEntity = _entityRepository.Query().FirstOrDefault(x => x.Id == entity.Id);
            if (oldEntity != null)
            {
                using (var txn = _entityRepository.BeginTransaction())
                {
                    CopyNewData(oldEntity, entity);
                    _entityRepository.Edit(oldEntity);
                    _entityRepository.SaveChange();
                    txn.Commit();
                }
            }

            return entity;
        }

        public void Remove(long entityId)
        {
            var entity = _entityRepository.Query().FirstOrDefault(x => x.Id == entityId);
            if (entity != null)
            {
                entity.Status = EntityStatus.Deleted;
                _entityRepository.Edit(entity);
                _entityRepository.SaveChange();
            }
        }

        public List<NeCategory> LoadAll(bool isActive = true, int status = -1, string name = "", bool isLikeSearch = false)
        {
            return _entityRepository.LoadAll(isActive, status, name, isLikeSearch);
        }

        public void DeletePermanently(long entityId)
        {
            var entity = _entityRepository.Query().FirstOrDefault(x => x.Id == entityId);
            if (entity != null)
            {
                _entityRepository.Remove(entity);
                _entityRepository.SaveChange();
            }
        }

        private void CopyNewData(NeCategory oldEntity, NeCategory entity)
        {
            oldEntity.ModificationDate = entity.ModificationDate;
            oldEntity.ModifyBy = entity.ModifyBy;
            oldEntity.Name = entity.Name;
            oldEntity.Status = entity.Status;
        }
    }
}