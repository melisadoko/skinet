using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec) 
        {
            if (spec.Criteria != null) 
            { 
                query = query.Where(spec.Criteria); // x=> x.SomeProperty == someValue
            }
            if(spec.OrderBy != null) 
            {
                query = query.OrderBy(spec.OrderBy); // x => x.SomeProperty
            }
            if (spec.OrderByDesc != null) 
            {
                query = query.OrderByDescending(spec.OrderByDesc); // x => x.SomeProperty
            }

            return query;
        }
    }
}
