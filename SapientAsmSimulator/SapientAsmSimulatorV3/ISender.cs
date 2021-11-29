// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ISender.cs$
// <copyright file="ISender.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices
{
    /// <summary>
    /// Interface for sending messages
    /// </summary>
    public interface ISender
    {
        /// <summary>
        /// Method to read and send xml from file.
        /// </summary>
        /// <param name="input_filename">Path to XML file to be loaded.</param>
        void ReadAndSendFile(string input_filename);
    }
}
