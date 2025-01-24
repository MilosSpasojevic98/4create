using ClinicalTrialApp.Features.ClinicalTrials.Commands;
using ClinicalTrialApp.Features.ClinicalTrials.Queries;
using ClinicalTrialApp.Models;
using ClinicalTrialApp.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalTrialApp.Endpoints;

public static class ClinicalTrialEndpoints
{
    public static void MapClinicalTrialEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trials")
            .WithTags("Clinical Trials")
            .WithOpenApi();

        group.MapPost("/upload", async Task<IResult> (
            IFormFile file,
            ISender mediator) =>
        {
            try
            {
                var command = new UploadTrialDataCommand(file);
                var result = await mediator.Send(command);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(new
                {
                    Message = "Trial data processed successfully",
                    TrialId = result.Value
                });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors);
            }
        })
        .DisableAntiforgery()
        .WithName("UploadTrialData")
        .WithDescription("Upload a JSON file containing clinical trial metadata")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id}", async Task<IResult> (
            Guid id,
            ISender mediator) =>
        {
            var query = new GetTrialByIdQuery(id);
            var trial = await mediator.Send(query);
            return trial is null ? Results.NotFound() : Results.Ok(trial);
        })
        .WithName("GetTrialById")
        .WithDescription("Get a clinical trial by its ID")
        .Produces<ClinicalTrialMetadata>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async Task<IResult> (
            [FromQuery] TrialStatus? status,
            ISender mediator) =>
        {
            var query = new GetTrialsQuery(status);
            var trials = await mediator.Send(query);
            return Results.Ok(trials);
        })
        .WithName("GetTrials")
        .WithDescription("Get all clinical trials, optionally filtered by status")
        .Produces<IEnumerable<ClinicalTrialMetadata>>(StatusCodes.Status200OK);
    }
}