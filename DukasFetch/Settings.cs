// ********************************************************
// Copyright (C) 2021 Louis S. Berman (louis@squideyes.com) 
// 
// This file is part of DukasFetch
// 
// The use of this source code is licensed under the terms 
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Trading.Context;

namespace DukasFetch
{
    public class Settings
    {
        public string? Folder { get; set; }
        public List<Symbol>? Symbols { get; set; }
        public int MinYear { get; set; }
        public bool Replace { get; set; } = true;
    }
}
