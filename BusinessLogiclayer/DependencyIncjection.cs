﻿using BusinessLogiclayer.Mappers;
using BusinessLogiclayer.ServiceContracts;
using BusinessLogiclayer.Services;
using BusinessLogiclayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrderMicorservice.BusinessLogicLayer;
public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        services.AddAutoMapper(typeof(OrderAddRequestToOderMappingProfile).Assembly);
        services.AddScoped<IOrdersService, OrdersService>();

        return services;
    }
}
