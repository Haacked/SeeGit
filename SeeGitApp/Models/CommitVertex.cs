﻿using System;
using System.Diagnostics;
using SeeGit.Models;

namespace SeeGit
{
    [DebuggerDisplay("{Sha}: {Message}")]
    public class CommitVertex : GitObject<CommitVertex>, IEquatable<CommitVertex>
    {
        public CommitVertex(string sha, string message)
        {
            Sha = sha;
            Message = message;
            Branches = new BranchCollection();
            Branches.CollectionChanged += (o, e) => RaisePropertyChanged(() => HasBranches);
            ShaLength = MainWindow.Configuration.GetSetting<int>("SHALength", 8);
            DescriptionShown = MainWindow.Configuration.GetSetting<bool>("DescriptionInExpander", false);
            AdornerMessageVisibilityType = MainWindow.Configuration.GetSetting<string>("AdornerCommitMessageVisibility", "ExpandedHidden");
            Expanded = false;
        }

        // Settings
        int _shaLength;
        public int ShaLength
        {
            get
            {
                return _shaLength;
            }
            set
            {
                _shaLength = value;
                RaisePropertyChanged(() => ShortSha);
            }
        }

        bool _descriptionShown;
        public bool DescriptionShown
        {
            get
            {
                return _descriptionShown;
            }
            set
            {
                _descriptionShown = value;
                RaisePropertyChanged(() => DescriptionShown);
            }
        }

        public bool AdornerMessageVisibility
        {
            get;
            set;
        }

        private string _adornerMessageVisibilityType;
        public string AdornerMessageVisibilityType
        {
            set
            {
                if (value.Equals("Visible"))
                    AdornerMessageVisibility = true;
                else if (value.Equals("Hidden"))
                    AdornerMessageVisibility = false;
                _adornerMessageVisibilityType = value;
                RaisePropertyChanged(() => AdornerMessageVisibility);
            }
        }

        public string Sha
        {
            get;
            private set;
        }

        public string ShortSha
        {
            get
            {
                return Sha.AtMost(ShaLength);
            }
        }

        public string Message
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            set;
        }

        public BranchCollection Branches
        {
            get;
            private set;
        }

        public bool HasBranches
        {
            get
            {
                return Branches.Count > 0;
            }
        }

        private bool _onCurrentBranch;

        public bool OnCurrentBranch
        {
            get
            {
                return _onCurrentBranch;
            }
            set
            {
                _onCurrentBranch = value;
                RaisePropertyChanged(() => OnCurrentBranch);
            }
        }

        public bool Expanded
        {
            set
            {
                if (_adornerMessageVisibilityType.Equals("ExpandedHidden"))
                {
                    AdornerMessageVisibility = !value;
                    RaisePropertyChanged(() => AdornerMessageVisibility);
                }
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CommitVertex);
        }

        public override bool Equals(CommitVertex other)
        {
            if(ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;
            return Equals(other.Sha, Sha);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", ShortSha, Message);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Sha != null ? Sha.GetHashCode() : 0);
                result = (result * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                result = (result * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(CommitVertex commit, CommitVertex other)
        {
            if(ReferenceEquals(commit, null)) return ReferenceEquals(other, null);

            return commit.Equals(other);
        }

        public static bool operator !=(CommitVertex commit, CommitVertex other)
        {
            return !(commit == other);
        }
    }
}