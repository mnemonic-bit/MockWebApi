using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class DemoMiddleware
    {

        private enum Operation { Add, Sub, Mul, Div }

        private static readonly Dictionary<Operation, Func<int, int, int>> OperationMethods = new Dictionary<Operation, Func<int, int, int>>()
        {
            [Operation.Add] = (int op1, int op2) => op1 + op2,
            [Operation.Sub] = (int op1, int op2) => op1 - op2,
            [Operation.Mul] = (int op1, int op2) => op1 * op2,
            [Operation.Div] = (int op1, int op2) => op1 / op2
        };

        private static readonly Random _random = new Random();

        public DemoMiddleware(RequestDelegate next)
        {

        }

        public async Task InvokeAsync(HttpContext context)
        {
            (Operation op, int min, int max)? maybeValues = ParsePath(context);

            if (!maybeValues.HasValue)
            {
                context.Response.StatusCode = 400;
                return;
            }

            (Operation op, int min, int max) = maybeValues.Value;

            int? value = CalculateValue(op, min, max);

            if (!value.HasValue)
            {
                context.Response.StatusCode = 400;
                return;
            }

            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(value.Value.ToString());
        }

        private static int GetRandomValue(int min, int max)
        {
            return min < max ? _random.Next(min, max) : _random.Next(max, min);
        }

        private static (Operation, int, int)? ParsePath(HttpContext context)
        {
            PathString path = context.Request.Path;
            if (!path.HasValue)
            {
                return null;
            }

            string[] segments = path.Value.Split('/');

            if (segments.Length != 4)
            {
                return null;
            }

            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(segments[0]));

            if (!Operation.TryParse(segments[1], out Operation op))
            {
                return null;
            }

            if (!int.TryParse(segments[2], out int op1))
            {
                return null;
            }

            if (!int.TryParse(segments[3], out int op2))
            {
                return null;
            }

            return (op, op1, op2);
        }

        private static int? CalculateValue(Operation op, int op1, int op2)
        {
            if(!OperationMethods.TryGetValue(op, out Func<int, int, int> operationFn))
            {
                throw new InvalidOperationException($"The operator '{op}' is not implemented. Please fix this class.");
            }

            return operationFn(op1, op2);
        }

    }
}
