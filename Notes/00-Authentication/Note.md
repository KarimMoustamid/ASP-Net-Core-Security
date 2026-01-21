ASP.NET Core Security Summary
This document outlines key security topics for ASP.NET Core, focusing on authentication, authorization, and modern identity protocols.

1. Core Security Concepts
   Authentication: The process of verifying the identity of a user or system.
   Authorization: Determining what an authenticated user or system is allowed to do.
   The Need for Authentication: Prevents unauthorized access and ensures credentials are valid before performing actions like updating data.
   Issues with Simple Credentials: Sending user/password in URLs is insecure because they can be captured by packet sniffers, logged by APIs, or cached on devices.
2. Token-Based Authentication & JWT
   Token-Based Authentication: A process where a user signs in to an Authorization Server to receive an access token, which is then used for subsequent API requests.
   Claims: Statements about an entity (e.g., user role, audience, or expiration).
   JSON Web Token (JWT): A compact, URL-safe way to represent claims.
   JWT Structure:
   Header: Contains the signing algorithm and token type.
   Payload: Contains claims such as issuer (iss), audience (aud), subject (sub), and expiration (exp).
   Signature: Used to verify that the sender is who they say they are and that the message wasn't changed.
3. ASP.NET Core Authorization Types
   Role-Based: Checks if a user has a specific role.
   Claims-Based: Uses specific user or client attributes to make decisions.
   Policy-Based: Uses predefined authorization policies.
   Resource-Based: Decisions are made based on the specific resource being accessed.
4. Modern Identity Protocols
   OAuth 2.0: An industry-standard protocol for authorization, allowing applications to access resources on behalf of a user.
   Flows: Includes Authorization Code Flow (best for front-ends), Client Credentials Flow, and Resource Owner Password Credentials Flow.
   OpenID Connect (OIDC): An identity layer on top of OAuth 2.0 that allows clients to verify the identity of the end-user.
   Standard Scopes: Includes openid, profile, email, address, and phone.
5. Tools and Infrastructure
   Docker: Used to package and run applications in isolated containers.
   Docker Compose: A tool for defining and running multi-container Docker applications.
   Keycloak: An open-source identity and access management (IAM) tool used to learn OIDC and secure applications.

---

dotnet user-jwts create
![JWT Creation Command in .NET CLI](/Notes/Images/s1.png)

dotnet user-jwts print JWT-ID
![Print the JWT Token Command in .NET CLI](/Notes/Images/s2.png)

- https://jwt.ms/
- https://jwt.io/

![401 error](/Notes/Images/s3.png)

we need to add the token to the request
![postman](/Notes/Images/s4.png)

We completed the feature
![postman](/Notes/Images/s5.png)

Testing the Upsert Basket Feature
![terminal](/Notes/Images/s6.png)

Testing with postman
![postmap Put](/Notes/Images/s7.png)

Cheking the DB
![Database](/Notes/Images/s8.png)

Testing the Get Basket Request
![postmap GET](/Notes/Images/s9.png)

Postman Test
![postmap GET](/Notes/Images/s10.png)

Forbiden
![postmap GET](/Notes/Images/s11.png)

Adding a Role to the JWT
![postmap GET](/Notes/Images/s12.png)
