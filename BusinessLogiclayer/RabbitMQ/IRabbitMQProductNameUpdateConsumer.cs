﻿namespace BusinessLogiclayer.RabbitMQ;

public interface IRabbitMQProductNameUpdateConsumer
{
    void Consume();
    void Dispose();
}