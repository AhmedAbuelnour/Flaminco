namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class ProjectionSpecification<TSource, TProject> where TSource : notnull
                                                                    where TProject : notnull
    {
        public abstract IQueryable<TProject> ProjectTo(IQueryable<TSource> query);
    }

    public class XXX : ProjectionSpecification<Student, StudentModel>
    {
        public override IQueryable<StudentModel> ProjectTo(IQueryable<Student> query)
        {
            return query.Select(a => new StudentModel
            {

            });
        }
    }


    public class Student
    {

    }

    public class StudentModel
    {

    }
}
