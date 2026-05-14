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

import {
  ThingsBooksyModulesUsersApiRequestsSignInRequest,
  ThingsBooksyModulesUsersApiRequestsSignUpRequest,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Users<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Account
   * @name GetAccount
   * @request GET:/users/me
   * @secure
   */
  getAccount = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/users/me`,
      method: "GET",
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags Account
   * @name SignUp
   * @request POST:/users/sign-up
   * @secure
   */
  signUp = (
    data: ThingsBooksyModulesUsersApiRequestsSignUpRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/users/sign-up`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Account
   * @name SignIn
   * @request POST:/users/sign-in
   * @secure
   */
  signIn = (
    data: ThingsBooksyModulesUsersApiRequestsSignInRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/users/sign-in`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Account
   * @name SignOut
   * @request POST:/users/logout
   * @secure
   */
  signOut = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/users/logout`,
      method: "POST",
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name GetUser
   * @request GET:/users/{id}
   * @secure
   */
  getUser = (id: string, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/users/${id}`,
      method: "GET",
      secure: true,
      ...params,
    });
}
