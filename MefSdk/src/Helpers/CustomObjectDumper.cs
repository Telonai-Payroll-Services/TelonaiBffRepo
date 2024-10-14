﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using MeF.Client.Logging;

namespace MeF.Client.Helpers
{
    /// <summary>
    /// Provides support for dumping objects to the log.
    /// </summary>
    internal static class CustomObjectDumper
    {
        #region Parameter constants

        // Maximum length of object names to detect possibly infinite recursions.
        private const Int32 ObjectNameLengthConstraint = 1000;

        #endregion Parameter constants

        #region Dump format constants

        private const String ObjectNullFormat = "{0} = [null]";
        private const String ObjectStringRepresentationFormat = "{0} = '{1}'";
        private const String AbortMessageTemplate = "{0} = [aborting expansion - {1}]";

        #endregion Dump format constants

        #region Message constants

        private const String NameLooksLikePasswordMessage = "name looks like a password";
        private const String PossibleInfiniteRecursionMessage = "detected a possibly infinite recursion (object name length constraint reached)";
        private const String ObjectTreeBackReferenceMessage = "detected a cycle in the object graph back to {0}";
        private const String ObjectTreeCrossReferenceMessage = "object already dumped as {0}";
        private const String IndexedPropertyMessage = "unable to expand indexed property";
        private const String PropertyReadExceptionMessage = "reading caused an exception";

        #endregion Message constants

        #region Public entry method

        /// <summary>
        /// Dumps a object to the log. The method will detect cycles in the object graph
        /// and not run into an infinite loop. With the parameter dumpObjectsOnlyOnce the
        /// method can further be instructed not to dump a object again if it reoccurs in
        /// the object graph even if no cycle is formed.
        /// </summary>
        /// <param name="o">The object to dump.</param>
        /// <param name="name">The name of the opject to dump.</param>
        /// <param name="dumpObjectsOnlyOnce">True to dump every object only once even if it occurs
        /// multiple times in the object graph without forming a cycle.</param>
        public static void DumpObject(Object o, String name, Boolean dumpObjectsOnlyOnce)
        {
            CustomObjectDumper.DumpObject(o, name, new Dictionary<Object, String>(), dumpObjectsOnlyOnce);
        }

        #endregion Public entry method

        #region Main method

        /// <summary>
        /// Dumps a object to the log. The method will detect cycles in the object graph
        /// and not run into an infinite loop. With the parameter dumpObjectsOnlyOnce the
        /// method can further be instructed not to dump a object again if it reoccurs in
        /// the object graph even if no cycle if formed.
        /// </summary>
        /// <param name="o">The object to dump.</param>
        /// <param name="name">The name of the opject to dump.</param>
        /// <param name="visitedObjects">A dictonary containing already visited objects.</param>
        /// <param name="dumpObjectsOnlyOnce">True to dump every object only once even if it occurs
        /// multiple times in the object graph without forming a cycle.</param>
        private static void DumpObject(Object o, String name, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            if (CustomObjectDumper.HandleObjectIsNull(name, o)) return;

            if (CustomObjectDumper.HandleObjectTreeBackAndCrossReferences(name, o, visitedObjects, dumpObjectsOnlyOnce)) return;

            // Handle possible infinite recursion after all other aborting cases
            // because they will abort the expansion in a more meaningful way.
            // This constraint prevents from entering a infinite recursion in whole
            // class of nasty cases. For example if a object exposes a dynamically
            // constructed new instances of its type through one if its properties.
            // An example is the struct DateTime that has a property Date exposing
            // another DateTime instance. (This is just an example - DateTime is
            // actually handled in a different way.)
            if (CustomObjectDumper.HandlePossibleInfiniteRecursion(name)) return;

            // Add the object to the visited objects to allow detection of object tree
            // cross and back references.
            CustomObjectDumper.AddObjectToVisitedObjectsIfRequired(name, o, visitedObjects);

            CustomObjectDumper.HandleWellFormedCases(name, o, visitedObjects, dumpObjectsOnlyOnce);

            // If requested, remove object from visited objects to allow dumping it again.
            CustomObjectDumper.RemoveObjectFromVisitedObjectsIfRequested(o, visitedObjects, dumpObjectsOnlyOnce);
        }

        #endregion Main method

        #region Primary methods

        private static Boolean HandleObjectIsNull(String name, Object o)
        {
            if (o == null)
            {
                CustomObjectDumper.DumpObjectNull(name);

                return true;
            }
            else
            {
                return false;
            }
        }

        private static Boolean HandlePossibleInfiniteRecursion(String name)
        {
            if (name.Length > CustomObjectDumper.ObjectNameLengthConstraint)
            {
                CustomObjectDumper.DumpAbortMessage(name, CustomObjectDumper.PossibleInfiniteRecursionMessage);

                return true;
            }
            else
            {
                return false;
            }
        }

        private static Boolean HandleObjectTreeBackAndCrossReferences(String name, Object o, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            if ((o is ValueType) || (o is String))
            {
                return false;
            }
            else
            {
                if (visitedObjects.ContainsKey(o))
                {
                    if (dumpObjectsOnlyOnce)
                    {
                        CustomObjectDumper.DumpAbortMessage(name, CustomObjectDumper.ObjectTreeCrossReferenceMessage.InvariantFormat(visitedObjects[o]));
                    }
                    else
                    {
                        CustomObjectDumper.DumpAbortMessage(name, CustomObjectDumper.ObjectTreeBackReferenceMessage.InvariantFormat(visitedObjects[o]));
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void AddObjectToVisitedObjectsIfRequired(String name, Object o, IDictionary<Object, String> visitedObjects)
        {
            if (!((o is ValueType) || (o is String)))
            {
                visitedObjects[o] = name;
            }
        }

        private static void HandleWellFormedCases(String name, Object o, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            if (o.GetType().Name.Equals("Byte[]"))
            {
                //No need to dump huge attachments in Audit file
                return;
            }

            if ((o.GetType().IsPrimitive) || (o.GetType().IsEnum) || (o is String) || (o is Guid) || (o is DateTime))
            {
                CustomObjectDumper.DumpObjectStringRepresentation(name, o);
            }
            else if (o is IEnumerable)
            {
                CustomObjectDumper.DumpIEnumerable(name, o as IEnumerable, visitedObjects, dumpObjectsOnlyOnce);
            }
            else
            {
                CustomObjectDumper.DumpPublicInstanceFields(name, o, visitedObjects, dumpObjectsOnlyOnce);
                CustomObjectDumper.DumpPublicInstanceProperties(name, o, visitedObjects, dumpObjectsOnlyOnce);
            }
        }

        private static void RemoveObjectFromVisitedObjectsIfRequested(Object o, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            if (!dumpObjectsOnlyOnce)
            {
                visitedObjects.Remove(o);
            }
        }

        #endregion Primary methods

        #region Dumper methods

        private static void DumpAbortMessage(String name, String message)
        {
            CustomObjectDumper.LogMessage(CustomObjectDumper.AbortMessageTemplate.InvariantFormat(name, message));
        }

        private static void DumpObjectNull(string name)
        {
            //CustomObjectDumper.LogMessage(CustomObjectDumper.ObjectNullFormat.InvariantFormat(name));
        }

        private static void DumpObjectStringRepresentation(String name, Object o)
        {
            CustomObjectDumper.LogMessage(CustomObjectDumper.ObjectStringRepresentationFormat.InvariantFormat(name, o));
        }

        private static void DumpIEnumerable(String name, IEnumerable items, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            Int32 index = 0;

            foreach (Object item in items)
            {
                CustomObjectDumper.DumpObject(item, name.AppendIndex(index), visitedObjects, dumpObjectsOnlyOnce);

                index++;
            }
        }

        private static void DumpPublicInstanceFields(String name, Object o, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            foreach (FieldInfo fieldInfo in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                CustomObjectDumper.DumpInstanceField(o, name, fieldInfo, visitedObjects, dumpObjectsOnlyOnce);
            }
        }

        private static void DumpInstanceField(Object o, String name, FieldInfo fieldInfo, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            if (!fieldInfo.HasDoNotDumpAttribute())
            {
                CustomObjectDumper.DumpObject(fieldInfo.GetValue(o), name.AppendMember(fieldInfo.Name), visitedObjects, dumpObjectsOnlyOnce);
            }
        }

        private static void DumpPublicInstanceProperties(String name, Object o, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            foreach (PropertyInfo propertyInfo in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                CustomObjectDumper.DumpInstanceProperty(name, o, propertyInfo, visitedObjects, dumpObjectsOnlyOnce);
            }
        }

        private static void DumpInstanceProperty(String name, Object o, PropertyInfo propertyInfo, IDictionary<Object, String> visitedObjects, Boolean dumpObjectsOnlyOnce)
        {
            if (!propertyInfo.HasDoNotDumpAttribute() && propertyInfo.CanRead)
            {
                if (propertyInfo.IsIndexed())
                {
                    CustomObjectDumper.DumpAbortMessage(name.AppendEmptyIndex(), CustomObjectDumper.IndexedPropertyMessage);
                }
                else
                {
                    Object value;
                    try
                    {
                        value = propertyInfo.GetValue(o, null);
                    }
                    catch (Exception)
                    {
                        CustomObjectDumper.DumpAbortMessage(name.AppendMember(propertyInfo.Name), CustomObjectDumper.PropertyReadExceptionMessage);

                        return;
                    }

                    CustomObjectDumper.DumpObject(value, name.AppendMember(propertyInfo.Name), visitedObjects, dumpObjectsOnlyOnce);
                }
            }
        }

        #endregion Dumper methods

        #region Logger interface

        private static void LogMessage(String message)
        {
            Audit.Write(message);
        }

        #endregion Logger interface
    }

    #region Helper extension classes

    public static class ObjectDumperStringExtensions
    {
        private const String MemberSeparator = ".";
        private const String OpeningIndex = "[";
        private const String ClosingIndex = "]";

        public static String InvariantFormat(this String format, params Object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static String AppendMember(this String name, String member)
        {
            return new StringBuilder(name).
                Append(ObjectDumperStringExtensions.MemberSeparator).
                Append(member).
                ToString();
        }

        public static String AppendIndex(this String name, Int32 index)
        {
            return new StringBuilder(name).
                Append(ObjectDumperStringExtensions.OpeningIndex).
                Append(index).
                Append(ObjectDumperStringExtensions.ClosingIndex).
                ToString();
        }

        public static String AppendEmptyIndex(this String name)
        {
            return new StringBuilder(name).
                Append(ObjectDumperStringExtensions.OpeningIndex).
                Append(ObjectDumperStringExtensions.ClosingIndex).
                ToString();
        }
    }

    public static class ObjectDumperMemberInfoExtension
    {
        public static Boolean HasDoNotDumpAttribute(this MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(typeof(DoNotDumpAttribute), true).Length > 0;
        }
    }

    public static class ObjectDumperPropertyInfoExtension
    {
        public static Boolean IsIndexed(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetIndexParameters().Length > 0;
        }
    }

    #endregion Helper extension classes

    #region DoNotDumpAttribute

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DoNotDumpAttribute : Attribute { }

    #endregion DoNotDumpAttribute
}