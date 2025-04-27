using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DhruvEnterprises.Core
{
    public class CustomPredicate
    {
        private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod(@"Contains", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo StringStartsWithMethod = typeof(string).GetMethod(@"StartsWith", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo IntEqualsMethod = typeof(int).GetMethod(@"Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(int) }, null);
        private static readonly MethodInfo LongIntEqualsMethod = typeof(long).GetMethod(@"Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(long) }, null);
        private static readonly MethodInfo DecimalEqualsMethod = typeof(decimal).GetMethod(@"Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(decimal) }, null);
        private static readonly MethodInfo DateTimeEqualsMethod = typeof(DateTime).GetMethod(@"Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(DateTime) }, null);
        private static readonly MethodInfo ByteEqualsMethod = typeof(Byte).GetMethod(@"Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(Byte) }, null);
        private static readonly MethodInfo BooleanEqualsMethod = typeof(Boolean).GetMethod(@"Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(Boolean) }, null);

        public static Expression<Func<TDbType, bool>> BuildPredicate<TDbType>(DataTableServerSide searchCriteria, Type[] TDbChildTypes = null)
        {
            if (searchCriteria.multisearch == null && (string.IsNullOrWhiteSpace(searchCriteria.search.value) || string.IsNullOrEmpty(searchCriteria.search.value)))
                return PredicateBuilder.True<TDbType>();

            var predicate = PredicateBuilder.False<TDbType>();

            if (!string.IsNullOrWhiteSpace(searchCriteria.search.value) && !string.IsNullOrEmpty(searchCriteria.search.value))
            {
                foreach (var entityFieldName in searchCriteria.columns.Where(c => !string.IsNullOrEmpty(c.name)).Select(c => c.name))
                {
                    Type dbType = typeof(TDbType);
                    string[] dbFieldNames = entityFieldName.Split(',');
                    MemberInfo dbFieldMemberInfo = null;
                    PropertyInfo dbPopertyInfo = null;

                    foreach (var dbFieldName in dbFieldNames)
                    {
                        if (dbFieldName.Contains('.'))
                        {
                            string childDbFieldName = dbFieldName.Split('.').LastOrDefault();

                            //pk
                            string entityName = dbFieldName.Split('.').FirstOrDefault(); 

                            if (TDbChildTypes != null && TDbChildTypes.Count() > 0)
                            {
                                //pk
                                //foreach (var dbChildType in TDbChildTypes)
                               foreach (var dbChildType in TDbChildTypes.Where(c=>c.Name== entityName))
                                {
                                    dbFieldMemberInfo = dbChildType.GetMember(childDbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                                    dbPopertyInfo = dbChildType.GetProperty(childDbFieldName);

                                    if (dbFieldMemberInfo != null && dbPopertyInfo != null)
                                    {
                                        predicate = GetPredicate(searchCriteria.search.value, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, dbFieldName.Replace("." + childDbFieldName, "").Split('.'));
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            dbFieldMemberInfo = dbType.GetMember(dbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                            dbPopertyInfo = dbType.GetProperty(dbFieldName);

                            if (dbFieldMemberInfo == null && dbPopertyInfo == null && TDbChildTypes != null && TDbChildTypes.Count() > 0)
                            {
                                foreach (var dbChildType in TDbChildTypes)
                                {
                                    dbFieldMemberInfo = dbChildType.GetMember(dbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                                    dbPopertyInfo = dbChildType.GetProperty(dbFieldName);

                                    if (dbFieldMemberInfo != null && dbPopertyInfo != null)
                                        predicate = GetPredicate(searchCriteria.search.value, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType);
                                }
                            }
                            else
                                predicate = GetPredicate(searchCriteria.search.value, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate);
                        }
                    }
                }
            }

            if (searchCriteria.multisearch != null)
            {
                predicate = PredicateBuilder.True<TDbType>();

                Type dbType = typeof(TDbType);
                MemberInfo dbFieldMemberInfo = null;
                PropertyInfo dbPopertyInfo = null;

                foreach (var dbSearchField in searchCriteria.multisearch)
                {
                    if (dbSearchField.column.Contains('.'))
                    {
                        string childDbFieldName = dbSearchField.column.Split('.').LastOrDefault();
                        //pk
                        string entityName = dbSearchField.column.Split('.').FirstOrDefault();

                        if (TDbChildTypes != null && TDbChildTypes.Count() > 0)
                        {
                            //pk
                            // foreach (var dbChildType in TDbChildTypes)
                            foreach (var dbChildType in TDbChildTypes.Where(e=>e.Name== entityName))
                            {
                                dbFieldMemberInfo = dbChildType.GetMember(childDbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                                dbPopertyInfo = dbChildType.GetProperty(childDbFieldName);

                                if (dbFieldMemberInfo != null && dbPopertyInfo != null)
                                    predicate = GetPredicate(dbSearchField.value, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, dbSearchField.column.Replace("." + childDbFieldName, "").Split('.'), dbSearchField.filter, dbSearchField.withOr);
                            }
                        }
                    }
                    else
                    {
                        dbFieldMemberInfo = dbType.GetMember(dbSearchField.column, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                        dbPopertyInfo = dbType.GetProperty(dbSearchField.column);

                        if (dbFieldMemberInfo == null && dbPopertyInfo == null && TDbChildTypes != null && TDbChildTypes.Count() > 0)
                        {
                            foreach (var dbChildType in TDbChildTypes)
                            {
                                dbFieldMemberInfo = dbChildType.GetMember(dbSearchField.column, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                                dbPopertyInfo = dbChildType.GetProperty(dbSearchField.column);

                                if (dbFieldMemberInfo != null && dbPopertyInfo != null)
                                    predicate = GetPredicate(dbSearchField.value, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, filterType: dbSearchField.filter, withOr: dbSearchField.withOr);
                            }
                        }
                        else
                            predicate = GetPredicate(dbSearchField.value, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, filterType: dbSearchField.filter, withOr: dbSearchField.withOr);
                    }
                }
            }

            return predicate;
        }

        private static Expression<Func<TDbType, bool>> GetPredicate<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            if (dbPopertyInfo.PropertyType == typeof(string))
                predicate = ApplyStringCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);
            else if (dbPopertyInfo.PropertyType == typeof(int) || dbPopertyInfo.PropertyType == typeof(int?))
                predicate = ApplyIntCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);
            
            //pk added for long type
            else if (dbPopertyInfo.PropertyType == typeof(long) || dbPopertyInfo.PropertyType == typeof(long?))
                predicate = ApplyLongIntCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);

            else if (dbPopertyInfo.PropertyType == typeof(decimal) || dbPopertyInfo.PropertyType == typeof(decimal?))
                predicate = ApplyDecimalCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);
            else if (dbPopertyInfo.PropertyType == typeof(DateTime) || dbPopertyInfo.PropertyType == typeof(DateTime?))
                predicate = ApplyDateTimeCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);
            else if (dbPopertyInfo.PropertyType == typeof(Byte) || dbPopertyInfo.PropertyType == typeof(Byte?))
                predicate = ApplyByteCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);
            else if (dbPopertyInfo.PropertyType == typeof(Boolean) || dbPopertyInfo.PropertyType == typeof(Boolean?))
                predicate = ApplyBooleanCriterion(searchCriteria, dbPopertyInfo, dbType, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);

            return predicate;
        }

        private static Expression<Func<TDbType, bool>> ApplyStringCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria) || string.IsNullOrEmpty(searchCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            var exprPred = GetMemeberExpression<TDbType>(searchCriteria, dbType, dbTypeParameter, dbFieldMemberInfo, predicate, dbChildType, childTypeNames, filterType, withOr);

            if (exprPred.Key != null)
                dbFieldMember = exprPred.Key;
            else
                return exprPred.Value;

            var criterionConstant = new Expression[] { Expression.Constant(searchCriteria) };

            MethodCallExpression methodCall = null;
            if (filterType == DataTableFilterType.StartsWith)
                methodCall = Expression.Call(dbFieldMember, StringStartsWithMethod, criterionConstant);
            else
                methodCall = Expression.Call(dbFieldMember, StringContainsMethod, criterionConstant);

            var lambda = Expression.Lambda(methodCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;

            if (lambda == null)
                return predicate;

            if (!withOr)
                return predicate.And(lambda);

            return predicate.Or(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyIntCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            int searchIntCriteria = 0;

            if (!int.TryParse(searchCriteria, out searchIntCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {

                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var criterionConstant = Expression.Constant(searchIntCriteria);

            Expression<Func<TDbType, bool>> lambda = null;

            if (dbPopertyInfo.PropertyType == typeof(int?))
            {
                var hasValueExpression = Expression.Property(dbFieldMember, "HasValue");
                var valueExpression = Expression.Property(dbFieldMember, "Value");
                var exp = Expression.Equal(valueExpression, criterionConstant);
                lambda = Expression.Lambda(exp, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }
            else
            {
                var methodCall = Expression.Call(dbFieldMember, IntEqualsMethod, criterionConstant);
                lambda = Expression.Lambda(methodCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }

            if (lambda == null)
                return predicate;

            if (!withOr)
                return predicate.And(lambda);

            return predicate.Or(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyLongIntCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            long searchIntCriteria = 0;

            if (!long.TryParse(searchCriteria, out searchIntCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {

                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var criterionConstant = Expression.Constant(searchIntCriteria);

            Expression<Func<TDbType, bool>> lambda = null;

            if (dbPopertyInfo.PropertyType == typeof(long?))
            {
                var hasValueExpression = Expression.Property(dbFieldMember, "HasValue");
                var valueExpression = Expression.Property(dbFieldMember, "Value");
                var exp = Expression.Equal(valueExpression, criterionConstant);
                lambda = Expression.Lambda(exp, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }
            else
            {
                var methodCall = Expression.Call(dbFieldMember, LongIntEqualsMethod, criterionConstant);
                lambda = Expression.Lambda(methodCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }

            if (lambda == null)
                return predicate;

            if (!withOr)
                return predicate.And(lambda);

            return predicate.Or(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyByteCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            Byte searchIntCriteria = 0;

            if (!Byte.TryParse(searchCriteria, out searchIntCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {

                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var criterionConstant = Expression.Constant(searchIntCriteria);

            Expression<Func<TDbType, bool>> lambda = null;

            if (dbPopertyInfo.PropertyType == typeof(Byte?))
            {
                var hasValueExpression = Expression.Property(dbFieldMember, "HasValue");
                var valueExpression = Expression.Property(dbFieldMember, "Value");
                var exp = Expression.Equal(valueExpression, criterionConstant);
                lambda = Expression.Lambda(exp, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }
            else
            {
                var methodCall = Expression.Call(dbFieldMember, ByteEqualsMethod, criterionConstant);
                lambda = Expression.Lambda(methodCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }

            if (lambda == null)
                return predicate;

            if (!withOr)
                return predicate.And(lambda);

            return predicate.Or(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyBooleanCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            Boolean searchIntCriteria = false;

            if (!Boolean.TryParse(searchCriteria, out searchIntCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {

                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var criterionConstant = Expression.Constant(searchIntCriteria);

            Expression<Func<TDbType, bool>> lambda = null;

            if (dbPopertyInfo.PropertyType == typeof(Boolean?))
            {
                var hasValueExpression = Expression.Property(dbFieldMember, "HasValue");
                var valueExpression = Expression.Property(dbFieldMember, "Value");
                var exp = Expression.Equal(valueExpression, criterionConstant);
                lambda = Expression.Lambda(exp, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }
            else
            {
                var methodCall = Expression.Call(dbFieldMember, BooleanEqualsMethod, criterionConstant);
                lambda = Expression.Lambda(methodCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }

            if (lambda == null)
                return predicate;

            if (!withOr)
                return predicate.And(lambda);

            return predicate.Or(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyDecimalCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            decimal searchDecimalCriteria = 0;

            if (!decimal.TryParse(searchCriteria, out searchDecimalCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {

                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var criterionConstant = Expression.Constant(searchDecimalCriteria);

            Expression<Func<TDbType, bool>> lambda = null;

            if (dbPopertyInfo.PropertyType == typeof(decimal?))
            {
                var hasValueExpression = Expression.Property(dbFieldMember, "HasValue");
                var valueExpression = Expression.Property(dbFieldMember, "Value");
                var exp = Expression.Equal(valueExpression, criterionConstant);
                lambda = Expression.Lambda(exp, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }
            else
            {
                var methodCall = Expression.Call(dbFieldMember, DecimalEqualsMethod, criterionConstant);
                lambda = Expression.Lambda(methodCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            }

            if (lambda == null)
                return predicate;

            if (!withOr)
                return predicate.And(lambda);

            return predicate.Or(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyDateTimeCriterion<TDbType>(string searchCriteria, PropertyInfo dbPopertyInfo, Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            DateTime searchDateTimeCriteria = DateTime.Now;

            if (!DateTime.TryParse(searchCriteria, out searchDateTimeCriteria))
                return predicate;

            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {
                    PropertyInfo propertyInfo = dbType.GetProperty(dbChildType.Name);
                    if (propertyInfo == null)
                    {
                        propertyInfo = dbType.GetProperty(dbChildType.Name + "s");

                        if (propertyInfo == null)
                            propertyInfo = dbType.GetProperty(dbChildType.Name + "es");

                        if (propertyInfo == null)
                            return predicate;

                        leftSubModel = Expression.Property(dbTypeParameter, propertyInfo);

                        ParameterExpression dbTypeParameterInner = Expression.Parameter(dbChildType, "xx");
                        MemberExpression dbFieldMemberInner = Expression.MakeMemberAccess(dbTypeParameterInner, dbFieldMemberInfo);

                        var criterionConstantInner = Expression.Constant(searchDateTimeCriteria);

                        var lambdaInner = GetDateTimeLambda<TDbType>(dbTypeParameterInner, criterionConstantInner, dbPopertyInfo, dbFieldMemberInner, filterType);

                        var lambdaAny = GetAnyExpression<TDbType>(dbChildType, dbTypeParameter, leftSubModel, lambdaInner) as Expression<Func<TDbType, bool>>;

                        if (lambdaAny == null)
                            return predicate;

                        return predicate.Or(lambdaAny);
                    }
                    else
                    {
                        leftSubModel = Expression.Property(dbTypeParameter, propertyInfo);
                        dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                    }
                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var criterionConstant = Expression.Constant(searchDateTimeCriteria);

            Expression<Func<TDbType, bool>> lambda = null;

            lambda = GetDateTimeLambda<TDbType>(dbTypeParameter, criterionConstant, dbPopertyInfo, dbFieldMember, filterType) as Expression<Func<TDbType, bool>>;

            if (lambda == null)
                return predicate;

            return predicate.Or(lambda);
        }

        private static KeyValuePair<MemberExpression, Expression<Func<TDbType, bool>>> GetMemeberExpression<TDbType>(string searchCriteria, Type dbType, ParameterExpression dbTypeParameter, MemberInfo dbFieldMemberInfo,
            Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null, DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            MemberExpression dbFieldMember = null;
            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {
                    PropertyInfo propertyInfo = dbType.GetProperty(dbChildType.Name);
                    if (propertyInfo == null)
                    {
                        return new KeyValuePair<MemberExpression, Expression<Func<TDbType, bool>>>(null, GetInnerPredicate(propertyInfo, searchCriteria, dbType, dbTypeParameter, dbFieldMemberInfo, predicate,
                            dbChildType, childTypeNames, filterType, withOr));
                    }
                    else
                    {
                        leftSubModel = Expression.Property(dbTypeParameter, propertyInfo);
                        dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                    }
                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            return new KeyValuePair<MemberExpression, Expression<Func<TDbType, bool>>>(dbFieldMember, null);
        }

        private static Expression<Func<TDbType, bool>> GetInnerPredicate<TDbType>(PropertyInfo propertyInfo, string searchCriteria, Type dbType, ParameterExpression dbTypeParameter,
            MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate, Type dbChildType = null, string[] childTypeNames = null,
            DataTableFilterType filterType = DataTableFilterType.Equals, bool withOr = true)
        {
            propertyInfo = dbType.GetProperty(dbChildType.Name + "s");

            if (propertyInfo == null)
                propertyInfo = dbType.GetProperty(dbChildType.Name + "es");

            if (propertyInfo == null)
                return predicate;

            var leftSubModel = Expression.Property(dbTypeParameter, propertyInfo);

            ParameterExpression dbTypeParameterInner = Expression.Parameter(dbChildType, "xx");
            MemberExpression dbFieldMemberInner = Expression.MakeMemberAccess(dbTypeParameterInner, dbFieldMemberInfo);

            var criterionConstantInner = new Expression[] { Expression.Constant(searchCriteria) };
            MethodCallExpression methodCallInner = null;

            if (filterType == DataTableFilterType.StartsWith)
                methodCallInner = Expression.Call(dbFieldMemberInner, StringStartsWithMethod, criterionConstantInner);
            else
                methodCallInner = Expression.Call(dbFieldMemberInner, StringContainsMethod, criterionConstantInner);

            var lambdaInner = Expression.Lambda(methodCallInner, dbTypeParameterInner);

            var lambdaAny = GetAnyExpression<TDbType>(dbChildType, dbTypeParameter, leftSubModel, lambdaInner) as Expression<Func<TDbType, bool>>;

            if (lambdaAny == null)
                return predicate;

            if (!withOr)
                predicate.And(lambdaAny);

            return predicate.Or(lambdaAny);
        }

        private static LambdaExpression GetDateTimeLambda<TDbType>(ParameterExpression dbTypeParameter, ConstantExpression criterionConstant, PropertyInfo dbPopertyInfo, MemberExpression dbFieldMember, DataTableFilterType filterType)
        {
            LambdaExpression lambda = null;

            if (dbPopertyInfo.PropertyType == typeof(DateTime?))
            {
                var hasValueExpression = Expression.Property(dbFieldMember, "HasValue");
                var valueExpression = Expression.Property(dbFieldMember, "Value");

                if (filterType == DataTableFilterType.LessThanOrEqual)
                {
                    var exp = Expression.LessThanOrEqual(valueExpression, criterionConstant);
                    lambda = Expression.Lambda(exp, dbTypeParameter);
                }
                else if (filterType == DataTableFilterType.GreaterThanOrEqual)
                {
                    var exp = Expression.GreaterThanOrEqual(valueExpression, criterionConstant);
                    lambda = Expression.Lambda(exp, dbTypeParameter);
                }
                else
                {
                    var exp = Expression.Equal(valueExpression, criterionConstant);
                    lambda = Expression.Lambda(exp, dbTypeParameter);
                }
            }
            else
            {
                if (filterType == DataTableFilterType.LessThanOrEqual)
                {
                    var exp = Expression.LessThanOrEqual(dbFieldMember, criterionConstant);
                    lambda = Expression.Lambda(exp, dbTypeParameter);
                }
                else if (filterType == DataTableFilterType.GreaterThanOrEqual)
                {
                    var exp = Expression.GreaterThanOrEqual(dbFieldMember, criterionConstant);
                    lambda = Expression.Lambda(exp, dbTypeParameter);
                }
                else
                {
                    MethodCallExpression methodCall = Expression.Call(dbFieldMember, DateTimeEqualsMethod, criterionConstant);
                    lambda = Expression.Lambda(methodCall, dbTypeParameter);
                }
            }

            return lambda;
        }

        private static LambdaExpression GetAnyExpression<TDbType>(Type dbChildType, ParameterExpression dbTypeParameter, Expression leftSubModel, LambdaExpression lambdaInner)
        {
            Func<MethodInfo, bool> methodLambda = m => m.Name == "Any" && m.GetParameters().Length == 2;
            MethodInfo method = typeof(Enumerable).GetMethods().Where(methodLambda).Single().MakeGenericMethod(dbChildType);

            MethodCallExpression callAny = Expression.Call(method, leftSubModel, lambdaInner);
            return Expression.Lambda(callAny, dbTypeParameter);
        }

        public static IQueryable BuildOrderBy(IQueryable source, DataTableServerSide searchCriteria, Type[] childTypes = null)
        {
            DataTableOrder orderBy = searchCriteria.order.FirstOrDefault();

            if (orderBy != null)
            {
                int columnIndex = orderBy.column;

                DataTableColumns orderColumn = searchCriteria.columns[columnIndex];
                if (orderColumn != null)
                {
                    var dbType = source.ElementType;
                    string[] dbFieldNames = orderColumn.name.Split(',');
                    string sortType = orderBy.dir == "asc" ? "OrderBy" : "OrderByDescending";
                    MemberInfo dbFieldMemberInfo = null;

                    foreach (var dbFieldName in dbFieldNames)
                    {
                        if (dbFieldName.Contains('.'))
                        {
                            string childDbFieldName = dbFieldName.Split('.').LastOrDefault();

                            if (childTypes != null && childTypes.Count() > 0)
                            {
                                foreach (var dbChildType in childTypes)
                                {
                                    dbFieldMemberInfo = dbChildType.GetMember(childDbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();

                                    if (dbFieldMemberInfo != null)
                                    {
                                        source = GetOrderBy(source, dbType, dbFieldMemberInfo, sortType, dbChildType, dbFieldName.Replace("." + childDbFieldName, "").Split('.'));
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            dbFieldMemberInfo = dbType.GetMember(dbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();

                            if (dbFieldMemberInfo == null && childTypes != null && childTypes.Count() > 0)
                            {
                                foreach (var dbChildType in childTypes)
                                {
                                    dbFieldMemberInfo = dbChildType.GetMember(dbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();

                                    if (dbFieldMemberInfo != null)
                                        source = GetOrderBy(source, dbType, dbFieldMemberInfo, sortType, dbChildType);
                                }
                            }
                            else if (dbFieldMemberInfo != null)
                            {
                                source = GetOrderBy(source, dbType, dbFieldMemberInfo, sortType);
                            }
                        }
                    }
                }
            }

            return source;
        }

        public static IQueryable BuildOrderBy(IQueryable source, CustomOrderBy orderBy, CustomOrderBy thenBy = null, Type[] childTypes = null)
        {
            var dbFieldName = orderBy.name;

            var dbType = source.ElementType;
            string sortType = orderBy.dir == "asc" ? "OrderBy" : "OrderByDescending";

            var dbFieldMemberInfo = dbType.GetMember(dbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();

            if (dbFieldMemberInfo == null && childTypes != null)
            {
                foreach (var dbChildType in childTypes)
                {
                    dbFieldMemberInfo = dbChildType.GetMember(dbFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                    if (dbFieldMemberInfo != null)
                        source = GetOrderBy(source, dbType, dbFieldMemberInfo, sortType, dbChildType);
                }
            }
            else
                source = GetOrderBy(source, dbType, dbFieldMemberInfo, sortType);

            if (thenBy != null)
            {
                var dbThenFieldName = thenBy.name;

                var dbThenType = source.ElementType;
                string sortThenType = thenBy.dir == "asc" ? "ThenBy" : "ThenByDescending";

                var dbThenFieldMemberInfo = dbThenType.GetMember(dbThenFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();

                if (dbThenFieldMemberInfo == null && childTypes != null)
                {
                    foreach (var dbChildType in childTypes)
                    {
                        dbThenFieldMemberInfo = dbChildType.GetMember(dbThenFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();
                        if (dbThenFieldMemberInfo != null)
                            source = GetOrderBy(source, dbThenType, dbThenFieldMemberInfo, sortThenType, dbChildType);
                    }
                }
                else
                    source = GetOrderBy(source, dbThenType, dbThenFieldMemberInfo, sortThenType);
            }

            return source;
        }

        private static IQueryable GetOrderBy(IQueryable source, Type dbType, MemberInfo dbFieldMemberInfo, string sortType, Type dbChildType = null, string[] childTypeNames = null)
        {
            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            MemberExpression dbFieldMember = null;

            if (dbChildType != null && dbFieldMemberInfo.DeclaringType == dbChildType)
            {
                Expression leftSubModel = dbTypeParameter;
                if (childTypeNames != null && childTypeNames.Length > 0)
                {
                    foreach (var childType in childTypeNames)
                    {
                        if (dbType.GetProperty(childType) != null)
                        {
                            leftSubModel = Expression.Property(leftSubModel, dbType.GetProperty(childType));
                            dbType = dbType.GetProperty(childType).PropertyType;
                        }
                    }
                    dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                }
                else
                {
                    PropertyInfo propertyInfo = dbType.GetProperty(dbChildType.Name);
                    if (propertyInfo == null)
                    {
                        propertyInfo = dbType.GetProperty(dbChildType.Name + "s");

                        if (propertyInfo == null)
                            propertyInfo = dbType.GetProperty(dbChildType.Name + "es");

                        if (propertyInfo == null)
                            return source;

                        leftSubModel = Expression.Property(dbTypeParameter, propertyInfo);

                        MethodCallExpression callFirstOrDefault = Expression.Call(typeof(Enumerable), "FirstOrDefault", new Type[] { dbChildType }, leftSubModel);

                        MemberExpression dbFieldMemberInner = Expression.MakeMemberAccess(callFirstOrDefault, dbFieldMemberInfo);

                        var lambdaOrderBy = Expression.Lambda(dbFieldMemberInner, dbTypeParameter);

                        if (lambdaOrderBy == null)
                            return source;

                        return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), sortType, new Type[] { source.ElementType, lambdaOrderBy.Body.Type }, source.Expression, lambdaOrderBy));
                    }
                    else
                    {
                        leftSubModel = Expression.Property(dbTypeParameter, propertyInfo);
                        dbFieldMember = Expression.Property(leftSubModel, dbChildType.GetProperty(dbFieldMemberInfo.Name));
                    }
                }
            }
            else
                dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);

            var lambda = Expression.Lambda(dbFieldMember, dbTypeParameter);

            if (lambda == null)
                return source;

            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), sortType, new Type[] { source.ElementType, lambda.Body.Type }, source.Expression, lambda));
        }

    }
}