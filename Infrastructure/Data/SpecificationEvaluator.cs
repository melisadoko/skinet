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

            if (spec.IsDistinct)
            {
                query = query.Distinct();
            }

            return query;
        }

        public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> query, ISpecification<T, TResult> spec)
        {
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria); // x=> x.SomeProperty == someValue
            }
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy); // x => x.SomeProperty
            }
            if (spec.OrderByDesc != null)
            {
                query = query.OrderByDescending(spec.OrderByDesc); // x => x.SomeProperty
            }

            var selectQuery = query as IQueryable<TResult>;

            if(spec.Select != null)
            {
                selectQuery = query.Select(spec.Select); // x => new { x.SomeProperty }
            }
            if(spec.IsDistinct)
            {
                selectQuery = selectQuery?.Distinct();
            }
            return selectQuery ?? query.Cast<TResult>();
        }
    }
}
