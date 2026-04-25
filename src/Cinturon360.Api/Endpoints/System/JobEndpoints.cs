using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Jobs.Commands;
using Cinturon360.Application.Features.Jobs.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.System;
using Cinturon360.Domain.Entities.System;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Api.Endpoints.System;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/system").WithTags("System").RequireAuthorization();

        group.MapPost("/jobs", EnqueueJob)
            .WithName("EnqueueJob")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapGet("/jobs/{jobId}", GetJobStatus)
            .WithName("GetJobStatus")
            .Produces<ApiResponse<JobResponse>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/documents", UploadDocument)
            .WithName("UploadDocument")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapGet("/documents", ListDocuments)
            .WithName("ListDocuments")
            .Produces<ApiResponse<IEnumerable<StoredDocumentResponse>>>(200);

        return app;
    }

    private static async Task<IResult> EnqueueJob([FromBody] EnqueueJobRequest request, ISender mediator)
    {
        if (!Enum.IsDefined(typeof(JobType), request.JobType))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("job.invalid_type", "Invalid job type.")));

        var result = await mediator.Send(new EnqueueJobCommand(
            (JobType)request.JobType,
            request.Payload,
            request.OrgId,
            request.UserId,
            request.ScheduledAt,
            request.MaxAttempts));

        return result.IsSuccess
            ? Results.Created($"/api/v1/system/jobs/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> GetJobStatus(string jobId, ISender mediator)
    {
        var result = await mediator.Send(new GetJobStatusQuery(jobId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("job.not_found", "Job not found.")))
            : Results.Ok(ApiResponse.Ok(MapJob(result.Value)));
    }

    private static async Task<IResult> UploadDocument([FromBody] UploadDocumentRequest request, ISender mediator)
    {
        if (!Enum.IsDefined(typeof(DocumentType), request.DocumentType))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("document.invalid_type", "Invalid document type.")));

        var result = await mediator.Send(new UploadDocumentCommand(
            (DocumentType)request.DocumentType,
            request.FileName,
            request.StorageKey,
            request.ContentType,
            request.FileSizeBytes,
            request.OrgId,
            request.UserId,
            request.SubjectId,
            request.IsPublic,
            request.ExpiresAt));

        return result.IsSuccess
            ? Results.Created($"/api/v1/system/documents/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> ListDocuments([FromQuery] string subjectId, ISender mediator)
    {
        var result = await mediator.Send(new ListDocumentsQuery(subjectId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(result.Value.Select(MapDocument)));
    }

    private static JobResponse MapJob(Job j) => new(
        j.Id,
        (int)j.JobType,
        (int)j.Status,
        j.OrgId,
        j.UserId,
        j.Payload,
        j.Attempts,
        j.MaxAttempts,
        j.ScheduledAt,
        j.StartedAt,
        j.CompletedAt,
        j.ErrorMessage);

    private static StoredDocumentResponse MapDocument(StoredDocument d) => new(
        d.Id,
        (int)d.DocumentType,
        d.FileName,
        d.StorageKey,
        d.ContentType,
        d.FileSizeBytes,
        d.IsPublic,
        d.CreatedAt,
        d.ExpiresAt);
}
