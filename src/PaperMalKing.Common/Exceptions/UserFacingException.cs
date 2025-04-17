// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

namespace PaperMalKing.Common.Exceptions;

public abstract class UserFacingException(string message) : TanteiException(message);