/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import { HttpClient, RequestParams } from "./http-client";

export class Ping<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags API
   * @name Pong
   * @request GET:/ping
   * @secure
   */
  pong = (params: RequestParams = {}) =>
    this.request<string, any>({
      path: `/ping`,
      method: "GET",
      secure: true,
      ...params,
    });
}
