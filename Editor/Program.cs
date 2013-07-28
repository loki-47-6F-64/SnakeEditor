using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Blocks;

namespace Editor
{
    static class Constant 
    {
        public const char chBlock = (char)9608;

        public const int iWindow_width = 100;
        public const int iWindow_height = 30;


        public const int iChange_shape = 001; //De cursor voor het veranderen van objects
        public const int iChange_color = 002; //De cursor voor het veranderen van de kleur van een object
        public const int iCopy = 003; //De cursor voor het kopieren van opjecten
        public const int iMove = 004; //De cursor voor het herpositioneren van een object
        public const int iResize = 005; //Een object vergroten/verkleinen
        public const int iErase = 006; //The mode for the removal of objects.
        public const int iMainMenu = 007; //The MainMenu
        public const int iEditMenu = 008; //The EditMenu
        public const int iMoveObject = 009; //The mode for moving all objects on the field.
        public const int iEditShape = 010;
        public const int iMoveStartPos = 011; //The mode for moving the startposition of the snake game.
        public const int iMoveTeleporter = 012;
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            
            field cField = new field();
            
            cField.input();
        }
    }

    class field
    {
        //De dll with the function to maximize the console
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        

        private bool bSaved = true;
        private int border_x, border_y; //De grenzen van het veld.
        public char[] chExtra_menu;

        private bool bExit;
        private int iLength_object_x = 1;
        private int iLength_object_y = 1;

        private pop_up box;
        private TextBox text;
        private Menu cEdit = new Menu();
        private Menu cMenu = new Menu();
        private group_block[] cField;
        private group_block cStartPosition;
        private group_block[] cTeleporter;
        private group_block cCursor;
        private Choise cChoise;

        private int MaxTeleport = 30; //The maximum number of teleporters
        private int NumberOfTeleport = 0;
        private bool bTeleportMoved = false; //Indicates wther or not the teleporter is moved using 'iMove'

        private int iMax_field = 20; //The size of the array 'cField'
        private string sBlock = new string(Constant.chBlock, 1);

        private int iMax_menu_item;
        private int iExtra_menu = 0; //The amount of added menu_items
        
        private int iMode; //De cursor die bewogen moet worden.

        private int iObject; //The object that needs to be moved.

        private int SizeX; //The sizes of the field.
        private int SizeY;

        private int object_item; //The amount of objects on the field.
        public field()
        {

            border_x = 21;
            border_y = Console.LargestWindowHeight - 7;

            iMax_menu_item = 10;

            //The extra Menu's
            chExtra_menu = new char[iMax_menu_item];
            File_manage cLoad_config = new File_manage(box);
            cLoad_config.load_menu(ref chExtra_menu, ref iMax_menu_item, ref iExtra_menu);
            bExit = false;
            cCursor = new group_block(border_x + 1, border_y - 1);
            cCursor.SetShape('+', 0, 0);
            new_field();
        }

        private void new_field()
        {
            Console.Title = "Unnamed field";

            box = new pop_up(border_x + 10, 10);
            box.SetColorText((int)ConsoleColor.Black, (int)ConsoleColor.Gray);
            box.SetColorWall((int)ConsoleColor.DarkYellow, 16);
            box.SetColorSign((int)ConsoleColor.Yellow);

            text = new TextBox(2, Console.LargestWindowHeight - 4, 15);
            text.SetColorBackground((int)ConsoleColor.Gray);
            text.SetColorText((int)ConsoleColor.Black);
            text.SetColorWall((int)ConsoleColor.DarkGray, 16);
            text.NewBox();

            
            Console.SetCursorPosition(0, Console.LargestWindowHeight - 6);
            for (int x = 0; x < Console.LargestWindowWidth; x++)
            {
                Console.Write("-");
            }
            for (int y = 0; y < Console.LargestWindowHeight; y++)
            {
                Console.SetCursorPosition(20, y);
                Console.Write("|");
            }
            object_item = 0; //Er is nog niks gebeurd op het veld.
            iMode = Constant.iMainMenu; //Beginnen met main_menu

            cField = new group_block[iMax_field];
            cTeleporter = new group_block[MaxTeleport];

            cStartPosition = new group_block(border_x + 1, border_y - 1);
            cStartPosition.SetShape('S', 0, 0);


            cMenu.CreateItem("Start edit", 4, 5);
            cMenu.CreateItem("New field",  4, 6);
            cMenu.CreateItem("Save field", 4, 7);
            cMenu.CreateItem("Load field", 4, 8);
            cMenu.CreateItem("Add shape",  4, 9);
            cMenu.CreateItem("Exit", 4, 11);
            

            string sShape_block = "Shape " + sBlock; //maak een string met het blokje
            cEdit.CreateItem("Remove object", 25, Console.LargestWindowHeight - 5);
            cEdit.CreateItem("change color", 25, Console.LargestWindowHeight - 4);
            cEdit.CreateItem("Edit object", 25, Console.LargestWindowHeight - 3);
            cEdit.CreateItem("Length horizontal", 25, Console.LargestWindowHeight - 2);
            cEdit.CreateItem("Length vertical", 25, Console.LargestWindowHeight - 1);

            cEdit.CreateItem("Copy object", 46, Console.LargestWindowHeight - 5);
            cEdit.CreateItem("Move object", 46, Console.LargestWindowHeight - 4);
            cEdit.CreateItem("Resize object", 46, Console.LargestWindowHeight - 3);
            cEdit.CreateItem("StartPosistion", 46, Console.LargestWindowHeight - 2);
            cEdit.CreateItem("Add Teleporter", 46, Console.LargestWindowHeight - 1);
            

            cEdit.CreateItem("Shape ]", 64, Console.LargestWindowHeight - 5);
            cEdit.CreateItem("Shape [", 64, Console.LargestWindowHeight - 4);
            cEdit.CreateItem("Shape <", 64, Console.LargestWindowHeight - 3);
            cEdit.CreateItem("Shape >", 64, Console.LargestWindowHeight - 2);
            cEdit.CreateItem("Shape _", 64, Console.LargestWindowHeight - 1);

            cEdit.CreateItem("Shape |", 79, Console.LargestWindowHeight - 5);
            cEdit.CreateItem(sShape_block, 79, Console.LargestWindowHeight - 4);
            cEdit.CreateItem("Shape #", 79, Console.LargestWindowHeight - 3);
            
            cMenu.SetCursor("<^>");
            cEdit.SetCursor("<^>");

            cEdit.ClearCursor();

            

            for (int x = 0; x < chExtra_menu.Length && chExtra_menu[x] != (char)0; x++)
            {
                int spot_x = cEdit.cMenu[cEdit.GetLength() - 1].pos_x; //vind een plaats voor de nieuwe menu_item
                int spot_y = cEdit.cMenu[cEdit.GetLength() - 1].pos_y + 1;

                if (spot_y >= Console.LargestWindowHeight)
                {
                    spot_x += 15;
                    spot_y = Console.LargestWindowHeight - 5;
                }

                string sLine = "Shape " + chExtra_menu[x].ToString();
                cEdit.CreateItem(sLine, spot_x, spot_y);
            }

            /* Because of a bug on the console of my computer I have to maximize the window here
             */
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 3); //Maximize de console.
            Console.SetCursorPosition(0, Console.LargestWindowHeight - 1);

            PrintAll();
        }
        public void input() //Input from the keyboard.
        {
            while (!bExit)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo Input;
                    Input = Console.ReadKey(true);

                    int iInstruction = 0;

                    switch (iMode)
                    {
                        case Constant.iMainMenu:
                            iInstruction = MainMenu.Process((int)Input.Key);
                            break;
                        case Constant.iEditMenu:
                            iInstruction = Edit.Process((int)Input.Key);
                            break;
                        case Constant.iCopy:
                        case Constant.iChange_shape:
                        case Constant.iChange_color:
                        case Constant.iMove:
                        case Constant.iResize:
                        case Constant.iErase:
                            iInstruction = ChangeObject.Process((int)Input.Key);
                            break;
                        case Constant.iMoveObject:
                            iInstruction = SetObject.Process((int)Input.Key);
                            break;
                        case Constant.iEditShape:
                            iInstruction = EditShape.Process((int)Input.Key);
                            break;
                        case Constant.iMoveStartPos:
                            iInstruction = StartPosition.Process((int)Input.Key);
                            break;
                        case Constant.iMoveTeleporter:
                            iInstruction = Teleport.Process((int)Input.Key);
                            break;

                    }
                    action(iInstruction);
                }
            }
        }
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private void EditMenuAction()
        {
            switch (cEdit.pos_cursor)
            {
                case 0: //eraser
                    cCursor.PrintShape();
                    iMode = Constant.iErase;
                    break;
                case 1: //change color
                    cCursor.PrintShape();
                    iMode = Constant.iChange_color;
                    break;
                case 2: //edit shape
                    cChoise = new Choise();
                    cChoise.chExtra_menu = chExtra_menu;
                    cChoise.new_menu();
                    iMode = Constant.iEditShape;
                    break;
                case 3://length horizontal
                    string sString_x = text.sWrite();
                    if (sString_x != "")
                    {
                        iLength_object_x = Function.StringToInt(sString_x);
                        if (iLength_object_x > SizeX)
                        {
                            box.Message("The size of the object is to great, I cannot contain it.");
                            iLength_object_x = 1;
                            PrintAll();
                        }
                    }
                    break;
                case 4://length vertical
                    string sString_y = text.sWrite();
                    if (sString_y != "")
                    {
                        iLength_object_y = Function.StringToInt(sString_y);
                        if (iLength_object_y > SizeY)
                        {
                            box.Message("The size of the object to great, I cannot contain it.");
                            iLength_object_y = 1;
                            PrintAll();
                        }
                    }
                    break;
                case 5: //copy
                    cCursor.PrintShape();
                    iMode = Constant.iCopy;
                    break;
                case 6: //move object
                    cCursor.PrintShape();
                    iMode = Constant.iMove;
                    break;
                case 7: //resize
                    cCursor.PrintShape();
                    iMode = Constant.iResize;
                    break;
                case 8: //StartPos
                    cStartPosition.PrintShape();
                    iMode = Constant.iMoveStartPos;
                    break;
                case 9: //Teleporter
                    //First give the teleporter an color
                    int BackColor = 0;
                    
                    for (int x = 0; x < NumberOfTeleport; x++)
                    {
                        if (BackColor == cTeleporter[x].GetBackColor())
                        {
                            BackColor++; //Try a new color
                            x = 0; //Reset the loop.
                        }
                    }
                    //Create an instance of the teleport. Use the the number of the teleport for the the background color.
                    cTeleporter[NumberOfTeleport] = new group_block(border_x + 1, border_y - 1);
                    cTeleporter[NumberOfTeleport].SetShape('@', 0, 0);
                    cTeleporter[NumberOfTeleport].SetColor(16, BackColor); //There is a pair of teleports for each color.
                    cTeleporter[NumberOfTeleport].PrintShape();
                    iObject = NumberOfTeleport;
                    NumberOfTeleport++;
                    bTeleportMoved = false;

                    iMode = Constant.iMoveTeleporter;
                    break;
                case 10:
                    edit(']');
                    break;
                case 11:
                    edit('[');
                    break;
                case 12:
                    edit('<');
                    break;
                case 13:
                    edit('>');
                    break;
                case 14:
                    edit('_');
                    break;
                case 15:
                    edit('|');
                    break;
                case 16:
                    edit(Constant.chBlock);
                    break;
                case 17:
                    edit('#');
                    break;
                //Put more actions for the Menu here.
                default:
                    if (cEdit.pos_cursor < cEdit.GetLength() &&
                        cEdit.pos_cursor - (cEdit.GetLength() - iExtra_menu) >= 0) //De extra menu_items
                    {
                        edit(chExtra_menu[cEdit.pos_cursor - cEdit.GetLength() + iExtra_menu]);
                    }
                    else
                    {
                        Console.Clear();
                        Console.Write("Er is geen actie gemaakt voor deze menu_item");
                        while (!Console.KeyAvailable) { }
                        Console.Clear();
                    }
                    break;
            }
        }
        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        ///
        
        private void MainMenuAction()
        {
            switch (cMenu.pos_cursor)
            {
                case 0: //Start edit
                    if (cField[0] == null)
                    {
                        box.Message("Create a new field before continuing");
                        break;
                    }
                    cMenu.ClearCursor();

                    cEdit.PrintMenu(cEdit.pos_cursor, cEdit.pos_cursor); //print the cursor

                    iMode = Constant.iEditMenu;
                    break;
                case 1: //Create a new field
                    if (cField[0] == null || box.Confirm("All unsaved progress will be discarded, are you sure you wish to continue?"))
                    {
                        //Verwijder de objects van het veld
                        for (int x = 0; x < cField.Length && cField[x] != null; x++)
                        {
                            cField[x].Clear();
                            cField[x] = null;
                        }
                        for (int x = 0; x < cTeleporter.Length && cTeleporter[x] != null; x++)
                        {
                            cTeleporter[x].Clear();
                            cTeleporter[x] = null;
                        }

                        cStartPosition.Clear();
                        cStartPosition.pos_x = border_x + 1;
                        cStartPosition.pos_y = border_y - 1;

                        object_item = 0; //There are no object on the field
                        iMax_field = 20;
                        Array.Resize(ref cField, iMax_field);//A new field

                        Console.Title = "Unnamed field";

                        int CheckX = Console.LargestWindowWidth - border_x;
                        int CheckY = Console.LargestWindowHeight - 7;
                        //Horizontal

                        box.Message("Please enter the horizontal length");
                        do
                        {
                            SizeX = Function.StringToInt(text.sWrite());

                            if (SizeX <= 1)
                            {
                                box.Message("Please enter a size greater than 1");
                            }
                            if (SizeX >= CheckX)
                            {
                                box.Message("The Field size is to great, I cannot contain it.");
                            }

                        } while (SizeX <= 1 || SizeX >= CheckX);

                        //Vertical
                        box.Message("Please enter the vertical length");
                        do
                        {
                            SizeY = Function.StringToInt(text.sWrite());

                            if (SizeY <= 1)
                            {
                                box.Message("Please enter a size greater than 1");
                            }
                            if (SizeY >= CheckY)
                            {
                                box.Message("The Field size is to great, I cannot contain it.");
                            }

                        } while (SizeY <= 1 || SizeY >= CheckY);

                        cField[0] = new group_block(border_x + 1, border_y - SizeY - 1, SizeX, 1);
                        cField[1] = new group_block(border_x + 1, border_y, SizeX, 1);
                        cField[2] = new group_block(border_x, border_y - SizeY - 1, 1, SizeY + 2);
                        cField[3] = new group_block(border_x + SizeX + 1, border_y - SizeY - 1, 1, SizeY + 2);

                        for (int z = 0; z < 4; z++)
                        {
                            for (int x = 0; x < cField[z].GetSize_x(); x++)
                            {
                                for (int y = 0; y < cField[z].GetSize_y(); y++)
                                {
                                    cField[z].SetShape(Constant.chBlock, x, y);
                                }
                            }
                        }
                        object_item = 4;
                    }
                    PrintAll();
                    break;
                case 2: //Save field
                    box.Message("Please enter the name of the file.");
                    string sFile_name = text.sWrite();
                    File_manage cSave = new File_manage(box);
                    if (cSave.Save_field(sFile_name, cField, cTeleporter, cStartPosition, object_item, NumberOfTeleport))
                    {
                        bSaved = true;
                        Console.Title = sFile_name;
                        box.Message("saving succesful");
                    }
                    else
                        box.Message("Saving not succesful");
                    for (int x = 0; x < object_item; x++)
                    {
                        cField[x].PrintShape();
                    }
                    //Saven
                    break;
                case 3: //load field
                    box.Message("Please enter the name of the file.");
                    string file_name = text.sWrite();
                    //clear the current objects on the field

                    for (int x = 0; x < object_item; x++)
                    {
                        cField[x].Clear();
                    }

                    File_manage cLoad = new File_manage(box);
                    if (cLoad.load_field(file_name, ref cField, ref cTeleporter, ref cStartPosition, ref object_item))
                    {
                        //The first of cField is the horizontal border
                        //The second is the vertical border.
                        SizeX = cField[0].GetSize_x();
                        SizeY = cField[2].GetSize_y() - 2;

                        //Figure out how many teleporters there are.
                        foreach (group_block x in cTeleporter)
                        {
                            if (x == null)
                            {
                                break;
                            }
                            NumberOfTeleport++;
                        }

                        iMax_field = cField.Length + 10;
                        Array.Resize(ref cField, iMax_field);
                        
                        bSaved = true;
                        Console.Title = file_name;
                        box.Message("Loading succesful");
                    }
                    else
                    {
                        box.Message("Loading not succesful");
                    }
                    PrintAll();
                    //Loading
                    break;
                case 4: //Add shape
                    //The new menu_item needs to fit in the console.
                    int spot_x = cEdit.cMenu[cEdit.GetLength() - 1].pos_x; //Find a spot for the new Menu item.
                    int spot_y = cEdit.cMenu[cEdit.GetLength() - 1].pos_y + 1;
                    if (spot_y >= Console.LargestWindowHeight)
                    {
                        spot_x += 15;
                        spot_y = Console.LargestWindowHeight - 5;
                    }
                    //7 represent the length of the string "Shape x"
                    if (spot_x + 7 < Console.LargestWindowWidth && iExtra_menu + 26 < border_y)
                    {
                        //If the decimal value of the shape is given, it needs to be converted to a char
                        //Else the first letter of the string needs to be converted to a char.
                        string sSymbol = text.sWrite();
                        if (sSymbol != "")
                        {
                            int iSymbol = Function.StringToInt(sSymbol); //Try to convert the string to an int.
                            char chSymbol;

                            if (iSymbol == -1) //The string contains a letter.
                            {
                                chSymbol = sSymbol[0];
                                sSymbol = "Shape " + sSymbol[0].ToString(); //Only the first letter of the string
                            }
                            else //The string only contains an number
                            {
                                chSymbol = (char)iSymbol;
                                sSymbol = "Shape " + chSymbol.ToString();
                            }

                            if (iExtra_menu >= iMax_menu_item) //Is the array big enough.
                            {
                                iMax_menu_item += 10;
                                Array.Resize(ref chExtra_menu, iMax_menu_item);
                            }
                            cEdit.CreateItem(sSymbol, spot_x, spot_y); //Add the new shape.

                            chExtra_menu[iExtra_menu] = chSymbol; //Let the program know what char has been added to the program.

                            iExtra_menu++; //Let the program know a new item has been added.
                        }
                    }
                    break;
                case 5: //exit
                    bExit = true;
                    if (!bSaved)
                    {
                        if (box.Confirm("Do you wish to save before exiting?"))
                        {
                            File_manage cFile = new File_manage(box);
                            if (!cFile.Save_field(text.sWrite(), cField, cTeleporter, cStartPosition, object_item, NumberOfTeleport))
                                if (!box.Confirm("Saving not succesful, do you wish to exit anyway?"))
                                {
                                    bExit = false; //abort exiting
                                }
                        }
                    }
                    File_manage cSave_menu = new File_manage(box);
                    cSave_menu.save_menu(chExtra_menu);
                    break;
                default:
                    Console.Clear();
                    Console.Write("Er is geen actie gemaakt voor deze menu_item");
                    while (!Console.KeyAvailable) { }
                    Console.Clear();
                    break;
            }
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private void action(int iInput)
        {
            //All of the data from the keyboard has been processed
            //Now we can put the program into some action.
            switch (iInput)
            {
                case Instruction.MainMenu_down:
                    cMenu.MoveCursor(1);
                    break;
                case Instruction.MainMenu_up:
                    cMenu.MoveCursor(-1);
                    break;
                case Instruction.MainMenu_enter:
                    MainMenuAction();
                    break;
                case Instruction.Edit_up:
                    cEdit.MoveCursor(-1);
                    break;
                case Instruction.Edit_down:
                    cEdit.MoveCursor(1);
                    break;
                case Instruction.Edit_left:
                    cEdit.MoveCursor(-5);
                    break;
                case Instruction.Edit_right:
                    cEdit.MoveCursor(5);
                    break;
                case Instruction.Edit_Escape: //Return to main Menu.
                    iMode = Constant.iMainMenu;
                    cEdit.ClearCursor();
                    cMenu.PrintMenu(cMenu.pos_cursor, cMenu.pos_cursor);
                    break;
                case Instruction.Edit_Delete:
                    if (cEdit.pos_cursor >= cEdit.GetLength() - iExtra_menu) //Only items that had been added later can be deleted.
                    {
                        cEdit.DeleteItem(cEdit.pos_cursor);

                        for (int x = cEdit.pos_cursor - 1 - (cEdit.GetLength() - iExtra_menu); x < iExtra_menu - 1; x++)
                        {
                            chExtra_menu[x] = chExtra_menu[x + 1];
                        }
                        chExtra_menu[iExtra_menu - 1] = (char)0;

                        if (cEdit.pos_cursor == cEdit.GetLength())
                        {
                            cEdit.MoveCursor(-1);
                        }
                        //The program needs to know an item had been deleted.
                        iExtra_menu--;
                    }
                    break;
                case Instruction.Edit_enter:
                    bSaved = false;
                    EditMenuAction();
                    break;
                case Instruction.MoveObject_up:
                case Instruction.MoveObject_down:
                case Instruction.MoveObject_left:
                case Instruction.MoveObject_right:
                    MoveObject(iInput);
                    break;
                case Instruction.MoveObject_enter:
                    //An object must not overlap the startposition. (for obvious reasons)
                    if (group_block.bCollision(cField[iObject],cStartPosition))
                    {
                        box.Message("An object cannot overlap the startposition");
                        PrintAll();
                        break;
                    }
                    bool bStop = false;
                    for (int iItem = 0; iItem < object_item; iItem++)
                    {
                        if (group_block.bCollision(cStartPosition,cField[iItem]))
                        {
                            box.Message("You cannot place the startposition over an object");
                            PrintAll();
                            bStop = true;
                            break;
                        }
                    }
                    if (bStop)
                    {
                        break;
                    }
                    iMode = Constant.iEditMenu;

                    cCursor.pos_x = cField[iObject].pos_x;
                    cCursor.pos_y = cField[iObject].pos_y;
                    if (iObject == object_item) //Has there a new instance of cField been added?
                    {
                        object_item++;
                    }
                    break;
                case Instruction.MoveObject_escape:
                    iMode = Constant.iEditMenu;
                    cField[iObject].Clear();
                    if (group_block.bCollision(cField[iObject],cStartPosition))
                    {
                        cStartPosition.PrintShape();
                    }
                    for (int iItem = 0; iItem < object_item; iItem++)
                    {
                        //If the object overwrites another object, then the overwritten object must be reprinted
                        if (group_block.bCollision(cField[iObject],cField[iItem]))
                        {
                            cField[iItem].PrintShape();
                        }
                    }
                    cCursor.pos_x = cField[iObject].pos_x;
                    cCursor.pos_y = cField[iObject].pos_y;
                    cField[object_item] = null;
                    break;
                case Instruction.Cursor_down:
                case Instruction.Cursor_up:
                case Instruction.Cursor_left:
                case Instruction.Cursor_right:
                    MoveCursor(iInput);
                    break;
                case Instruction.Cursor_enter:
                    switch (iMode)
                    {
                        case Constant.iErase:
                            EraseObject();
                            break;
                        case Constant.iCopy:
                            CopyObject();
                            break;
                        case Constant.iChange_color:
                            ChangeColor();
                            break;
                        case Constant.iChange_shape:
                            ChangeShape();
                            break;
                        case Constant.iMove:
                            ChangePosition();
                            cCursor.SetShape('+', 0, 0);//Return the shape of the cursor to the original shape.
                            break;
                        case Constant.iResize:
                            ChangeSize();
                            break;
                    }
                    break;
                case Instruction.Cursor_escape:
                    iMode = Constant.iEditMenu;
                    int ItemHit = 0;
                    bool ItemOverlapped = false;
                    for (int iItem = 0; iItem < object_item; iItem++)
                    {
                        //If the cursor overwrites another object, then the overwritten object must be reprinted
                        if (bOverlapped(iItem))
                        {
                            ItemHit = iItem;
                            ItemOverlapped = true;
                        }
                    }
                    if (ItemOverlapped)
                    {
                        cField[ItemHit].PrintShape();
                    }
                    else
                    {
                        cCursor.Clear();
                    }
                    cCursor.SetShape('+', 0, 0);
                    break;
                case Instruction.EditShape_up:
                    cChoise.cChoise.MoveCursor(-1);
                    break;
                case Instruction.EditShape_down:
                    cChoise.cChoise.MoveCursor(1);
                    break;
                case Instruction.EditShape_enter:
                    iMode = Constant.iChange_shape;
                    cChoise.cChoise.clear_menu();

                    cCursor.SetShape(cChoise.action(), 0, 0);
                    cChoise = null;
                    break;
                case Instruction.EditShape_escape:
                    cChoise.cChoise.clear_menu();
                    cChoise = null;
                    iMode = Constant.iEditMenu;
                    break;
                case Instruction.MoveStart_down:
                case Instruction.MoveStart_left:
                case Instruction.MoveStart_right:
                case Instruction.MoveStart_up:
                    MoveStartPos(iInput);
                    break;
                case Instruction.MoveStart_enter:
                    bool Break = false;
                    //The startposition cannot overlap an object.
                    for (int iItem = 0; iItem < object_item; iItem++)
                    {
                        if (group_block.bCollision(cStartPosition,cField[iItem]))
                        {
                            box.Message("You cannot place the startposition over an object");
                            PrintAll();
                            Break = true;
                            break;
                        }
                    }
                    //The teleporters cannot overlap an object.
                    for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
                    {
                        if (group_block.bCollision(cStartPosition,cTeleporter[iItem]))
                        {
                            box.Message("You cannot place the startposition over an teleporter");
                            PrintAll();
                            Break = true;
                            break;
                        }
                    }
                    if (Break)
                    {
                        break;
                    }
                    iMode = Constant.iEditMenu;
                    break;
                case Instruction.Teleport_up:
                case Instruction.Teleport_down:
                case Instruction.Teleport_left:
                case Instruction.Teleport_right:
                    MoveTeleporter(iInput);
                    break;
                case Instruction.Teleport_enter:
                    if(group_block.bCollision(cTeleporter[iObject],cStartPosition))
                    {
                        box.Message("The teleporter cannot overlap the startposition");
                        PrintAll();
                        break;
                    }
                    bool bBreak = false;
                    //The teleporters cannot overlap an object.
                    for (int iItem = 0; iItem < object_item; iItem++)
                    {
                        if (group_block.bCollision(cTeleporter[iObject],cField[iItem]))
                        {
                            box.Message("You cannot place the teleporter over an object");
                            PrintAll();
                            bBreak = true;
                            break;
                        }
                    }
                    if (bBreak)
                    {
                        break;
                    }
                    bool Even = (NumberOfTeleport / 2 == (double)NumberOfTeleport / 2);
                    if(Even || bTeleportMoved)
                    {
                        iMode = Constant.iEditMenu;
                    }
                    else
                    {
                        int BackColor = cTeleporter[NumberOfTeleport - 1].GetBackColor();
                        //Create an instance of the teleport. Use the the number of the teleport for the the background color.
                        cTeleporter[NumberOfTeleport] = new group_block(border_x + 1, border_y - 1);
                        cTeleporter[NumberOfTeleport].SetShape('@', 0, 0);
                        cTeleporter[NumberOfTeleport].SetColor(16, BackColor); //There is a pair of teleports for each color.
                        cTeleporter[NumberOfTeleport].PrintShape();
                        iObject = NumberOfTeleport;
                        NumberOfTeleport++;
                    }
                    break;
            }
        }
        //Print all the objects
        private void PrintAll()
        {
            cStartPosition.PrintShape();
            for (int x = 0; x < cTeleporter.Length && cTeleporter[x] != null; x++)
            {
                cTeleporter[x].PrintShape();
            }
            for (int x = 0; x < cField.Length && cField[x] != null; x++)
            {
                cField[x].PrintShape();
            }
            if (iMode == Constant.iMove)
            {
                cCursor.PrintShape();
            }
        }
        private void MoveStartPos(int iInstruction)
        {
            cStartPosition.Clear();

            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cStartPosition,cTeleporter[iItem]))
                {
                    cTeleporter[iItem].PrintShape();
                }
            }
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cStartPosition, cField[iItem]))
                {
                    cField[iItem].PrintShape();
                }
            } //The object cannot go over the borders of the field.
            if (iInstruction == Instruction.MoveStart_up && cStartPosition.pos_y > border_y - SizeY)
            {
                cStartPosition.pos_y--;
            }
            else if (iInstruction == Instruction.MoveStart_down && cStartPosition.pos_y + cStartPosition.GetSize_y() <= border_y - 1)
            {
                cStartPosition.pos_y++;
            }
            else if (iInstruction == Instruction.MoveStart_left && cStartPosition.pos_x > border_x + 1)
            {
                cStartPosition.pos_x--;
            }
            else if (iInstruction == Instruction.MoveStart_right && cStartPosition.pos_x + cStartPosition.GetSize_x() < border_x + SizeX + 1)
            {
                cStartPosition.pos_x++;
            }
            cStartPosition.PrintShape();
        }

        private void MoveTeleporter(int iInstruction)
        {
            cTeleporter[iObject].Clear();
            if (group_block.bCollision(cTeleporter[iObject],cStartPosition))
            {
                cStartPosition.PrintShape();
            }
            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cTeleporter[iObject],cTeleporter[iItem]) && iItem != iObject)
                {
                    cTeleporter[iItem].PrintShape();
                }
            }

            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cTeleporter[iObject],cField[iItem]))
                {
                    cField[iItem].PrintShape();
                }
            } //The object cannot go over the borders of the field.
            if (iInstruction == Instruction.Teleport_up && cTeleporter[iObject].pos_y > border_y - SizeY)
            {
                cTeleporter[iObject].pos_y--;
            }
            else if (iInstruction == Instruction.Teleport_down && cTeleporter[iObject].pos_y + cTeleporter[iObject].GetSize_y() <= border_y - 1)
            {
                cTeleporter[iObject].pos_y++;
            }
            else if (iInstruction == Instruction.Teleport_left && cTeleporter[iObject].pos_x > border_x + 1)
            {
                cTeleporter[iObject].pos_x--;
            }
            else if (iInstruction == Instruction.Teleport_right && cTeleporter[iObject].pos_x + cTeleporter[iObject].GetSize_x() < border_x + SizeX + 1)
            {
                cTeleporter[iObject].pos_x++;
            }
            cTeleporter[iObject].PrintShape();
        }

        private void MoveObject(int iInstruction)
        {
            cField[iObject].Clear();
            if(group_block.bCollision(cField[iObject],cStartPosition))
            {
                cStartPosition.PrintShape();
            }
            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cField[iObject],cTeleporter[iItem]))
                {
                    cTeleporter[iItem].PrintShape();
                }
            }
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cField[iObject],cField[iItem]) && iObject != iItem)
                {
                    cField[iItem].PrintShape();
                }
            } //The object cannot go over the borders of the field.
            if (iInstruction == Instruction.MoveObject_up && cField[iObject].pos_y > border_y - SizeY)
            {
                cField[iObject].pos_y--;
            }
            else if (iInstruction == Instruction.MoveObject_down && cField[iObject].pos_y + cField[iObject].GetSize_y() <= border_y - 1)
            {
                cField[iObject].pos_y++;
            }
            else if (iInstruction == Instruction.MoveObject_left && cField[iObject].pos_x > border_x + 1)
            {
                cField[iObject].pos_x--;
            }
            else if (iInstruction == Instruction.MoveObject_right && cField[iObject].pos_x + cField[iObject].GetSize_x() < border_x + SizeX + 1)
            {
                cField[iObject].pos_x++;
            }
            cField[iObject].PrintShape();
        }
        private void ChangePosition()
        {
            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                if (group_block.bCollision(cCursor,cTeleporter[iItem]))
                {
                    iObject = iItem;
                    iMode = Constant.iMoveTeleporter;
                    bTeleportMoved = true;
                }
            }
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                if (bOverlapped(iItem))
                {
                    //Change the position of the object if the object is not a border.
                    if (iItem < 4)
                    {
                        box.Message("You cannot change the position of the borders");
                        PrintAll();
                        return;
                    }
                    iObject = iItem;
                    iMode = Constant.iMoveObject;
                }
            }
        }
        private void ChangeShape()
        {
            bool bItemOverlapped = false;
            int iItemHit = 0;
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //If the cursor overwrites another object, then the overwritten object must be reprinted
                if (bOverlapped(iItem))
                {
                    iItemHit = iItem;
                    bItemOverlapped = true;
                }
            }
            if (bItemOverlapped)
            {
                cField[iItemHit].SetShape(cCursor.GetShape(0, 0), cCursor.pos_x - cField[iItemHit].pos_x,
                                                      cCursor.pos_y - cField[iItemHit].pos_y);
            }
        }
        //Erase an object and let other objects take its place int he array.
        private void EraseObject()
        {
            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                if (group_block.bCollision(cCursor,cTeleporter[iItem]))
                {
                    bool Even = (iItem / 2 == (double)iItem / 2);
                    if (Even)
                    {
                        cTeleporter[iItem].Clear();
                        cTeleporter[iItem + 1].Clear();
                        for (int x = iItem; x < MaxTeleport - 2 && cTeleporter[x] != null; x += 2)
                        {
                            cTeleporter[x] = cTeleporter[x + 2];
                            cTeleporter[x + 1] = cTeleporter[x + 3];
                        }
                    }
                    else
                    {
                        cTeleporter[iItem].Clear();
                        cTeleporter[iItem - 1].Clear();
                        for (int x = iItem; x < MaxTeleport - 2; x += 2)
                        {
                            cTeleporter[x] = cTeleporter[x + 2];
                            cTeleporter[x - 1] = cTeleporter[x + 1];
                        }
                    }
                    NumberOfTeleport -= 2;
                }
            }
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //Remove the selected object if the object is not a border.
                if (bOverlapped(iItem))
                {
                    if (iItem < 4)
                    {
                        box.Message("You cannot remove the borders of the field");
                        PrintAll();
                        return;
                    }
                    cField[iItem].Clear();
                    cCursor.PrintShape();
                    for (int x = iItem; x < cField.GetLength(0) - 1; x++)
                    {
                        cField[x] = cField[x + 1];
                    }
                    object_item--;
                }
            }
            cCursor.PrintShape();
        }
        private void CopyObject()
        {
            bool bItemOverlapped = false;
            int iItemHit = 0;
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                if (bOverlapped(iItem))
                {
                    //Copy the selected object if the object is not a border.
                    if (iItem < 4)
                    {
                        box.Message("You cannot copy the borders of the field");
                        PrintAll();
                        return;
                    }
                    iItemHit = iItem;
                    bItemOverlapped = true;
                }
            }
            if (bItemOverlapped)
            {
                if (object_item + 1 >= iMax_field)
                {
                    iMax_field += 10;
                    Array.Resize(ref cField, iMax_field);
                }
                int iLength_x = cField[iItemHit].GetSize_x();
                int iLength_y = cField[iItemHit].GetSize_y();
                int pos_x = cField[iItemHit].pos_x;
                int pos_y = cField[iItemHit].pos_y;
                int iText_color = cField[iItemHit].GetTextColor();
                int iBack_color = cField[iItemHit].GetBackColor();

                cField[object_item] = new group_block(pos_x, pos_y, iLength_x, iLength_y);
                cField[object_item].SetShape(cField[iItemHit].GetShape());

                cField[object_item].SetColor(iText_color, iBack_color);

                iObject = object_item;
                iMode = Constant.iMoveObject;
            }
        }
        private void ChangeSize()
        {
            int iItemHit = 0;
            bool bItemOverlapped = false;



            for (int iItem = 0; iItem < object_item; iItem++)
            {
                if (bOverlapped(iItem))
                {
                    //Change the size of the selected object if the object is not a border.
                    if (iItem < 4)
                    {
                        box.Message("You cannot change the size of the borders of the field");
                        PrintAll();
                        return;
                    }
                    iItemHit = iItem;
                    bItemOverlapped = true;
                }
            }
            if (bItemOverlapped)
            {
                int size_x;
                box.Message("Please enter the horizontal length");
                do
                {
                    size_x = Function.StringToInt(text.sWrite());
                    if (size_x <= 0)
                    {
                        box.Message("Please enter a size greater than 0.");
                    }
                    else if (size_x >= SizeX + 1)
                    {
                        box.Message("Please enter a size smaller than " + (SizeX + 1));
                    }
                } while (size_x <= 0 || size_x >= SizeX + 1);

                int size_y;
                box.Message("Please enter the vertical length");
                do
                {
                    size_y = Function.StringToInt(text.sWrite());
                    if (size_y <= 0)
                    {
                        box.Message("Please enter a size greater than 0.");
                    }
                    else if (size_y >= SizeY + 1)
                    {
                        box.Message("Please enter a size smaller than " + (SizeY + 1));
                    }
                } while (size_y <= 0 || size_y >= SizeY + 1);
                /* First initialise a new instance of group_block
                 * Then give the new the instance the shape of the old version
                 * If the new version is bigger than the old one, put blocks in the parts that were not covered with the old shape.
                 */
                cField[iItemHit].Clear();
                int pos_x = cField[iItemHit].pos_x;
                int pos_y = cField[iItemHit].pos_y;

                //Save the color.
                int iText_color = cField[iItemHit].GetTextColor();
                int iBack_color = cField[iItemHit].GetBackColor();
                //They cannot go over the borders.
                if (pos_x + size_x >= border_x + SizeX)
                {
                    pos_x = border_x + SizeX + 1 - size_x;
                }
                if (pos_y + size_y >= border_y - 1)
                {
                    pos_y = border_y - size_y;
                }
                //Save the old shape.
                char[][] chTemp_shape = cField[iItemHit].GetShape();

                cField[iItemHit] = new group_block(pos_x, pos_y, size_x, size_y);

                for (int x = 0; x < size_x; x++)
                {
                    for (int y = 0; y < size_y; y++)
                    {
                        cField[iItemHit].SetShape(Constant.chBlock, x, y);
                    }
                }
                for (int y = 0; y < chTemp_shape.Length && y < size_y; y++)
                {
                    for (int x = 0; x < chTemp_shape[y].Length && x < size_x; x++)
                    {
                        cField[iItemHit].SetShape(chTemp_shape[y][x], x, y);
                    }
                }
                //Give the new instance the same color as the old instance.
                cField[iItemHit].SetColor(iText_color, iBack_color);
                cField[iItemHit].PrintShape();
            }
        }
        private void ChangeColor()
        {
            string[] sColumn_color = {
                                                             "Foreground",
                                                             "Background"
                                                         };
            string[] sChoises = { 
                                        "Black",
                                        "Dark blue",
                                        "Dark green",
                                        "Teal",
                                        "Dark red",
                                        "Dark purple",
                                        "Gold",
                                        "Grey",
                                        "Dark grey",
                                        "Blue",
                                        "Green",
                                        "Cyan",
                                        "Red",
                                        "Purple",
                                        "Yellow",
                                        "White",
                                        "Default color"
                                                    };

            bool bItemOverlapped = false;
            int iItemHit = 0;
            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                if (group_block.bCollision(cCursor,cTeleporter[iItem]))
                {
                    int iColor = box.Choise("What color do you want", sChoises);
                    bool Even = (iItem / 2 == (double)iItem / 2);
                    if (Even) //If the item is even than the other teleporter is item + 1 else the other one is item - 1
                    {
                        cTeleporter[iItem].SetColor(16, iColor);
                        cTeleporter[iItem + 1].SetColor(16, iColor);
                    }
                    else
                    {
                        cTeleporter[iItem].SetColor(16, iColor);
                        cTeleporter[iItem - 1].SetColor(16, iColor);
                    }
                    PrintAll();
                }
            }
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //If the cursor overwrites another object, then the overwritten object must be reprinted
                if (bOverlapped(iItem))
                {
                    iItemHit = iItem;
                    bItemOverlapped = true;
                }
            }
            if (bItemOverlapped)
            {
                int iGround = box.Choise("What color do you wish to change?", sColumn_color);
                int iColor = box.Choise("What color do you want?", sChoises);

                //If the object that needs to be changed is an border, all the other borders need to be changed as well.
                if (iItemHit < 4)
                {
                    switch (iGround)
                    {
                        case 0:
                            cField[0].SetColor(iColor, cField[iItemHit].GetBackColor());
                            cField[1].SetColor(iColor, cField[iItemHit].GetBackColor());
                            cField[2].SetColor(iColor, cField[iItemHit].GetBackColor());
                            cField[3].SetColor(iColor, cField[iItemHit].GetBackColor());
                            break;
                        case 1:
                            cField[0].SetColor(cField[iItemHit].GetTextColor(), iColor);
                            cField[1].SetColor(cField[iItemHit].GetTextColor(), iColor);
                            cField[2].SetColor(cField[iItemHit].GetTextColor(), iColor);
                            cField[3].SetColor(cField[iItemHit].GetTextColor(), iColor);
                            break;
                    }
                }
                else //If the object is not a border, only the object in question needs to be changed.
                {
                    switch (iGround)
                    {
                        case 0:
                            cField[iItemHit].SetColor(iColor, cField[iItemHit].GetBackColor());
                            break;
                        case 1:
                            cField[iItemHit].SetColor(cField[iItemHit].GetTextColor(), iColor);
                            break;
                    }
                }
                PrintAll();
            }
        }
        private void MoveCursor(int iMovement)
        {
            int iItem_hit = 0;
            bool bItemOverlapped = false;
            if (group_block.bCollision(cCursor,cStartPosition))
            {
                cStartPosition.PrintShape();
                bItemOverlapped = true;
            }
            for (int iItem = 0; iItem < NumberOfTeleport; iItem++)
            {
                //If the object overwrites another object, then the overwritten object must be reprinted
                if (group_block.bCollision(cCursor,cTeleporter[iItem]))
                {
                    cTeleporter[iItem].PrintShape();
                    bItemOverlapped = true;
                }
            }
            for (int iItem = 0; iItem < object_item; iItem++)
            {
                //If the cursor overwrites another object, then the overwritten object must be reprinted
                if (bOverlapped(iItem))
                {
                    iItem_hit = iItem;
                    bItemOverlapped = true;
                }
            }
            if (!bItemOverlapped)
            {
                cCursor.Clear(); //The output on that part of the screen must be cleaned up.
            }
            else
            {
                cField[iItem_hit].PrintShape();
            }

            if (iMovement == Instruction.Cursor_up && cCursor.pos_y >= border_y - SizeY)
            {
                cCursor.pos_y--;
            }
            else if (iMovement == Instruction.Cursor_down && cCursor.pos_y + cCursor.GetSize_y() <= border_y)
            {
                cCursor.pos_y++;
            }
            else if (iMovement == Instruction.Cursor_left && cCursor.pos_x > border_x)
            {
                cCursor.pos_x--;
            }
            else if (iMovement == Instruction.Cursor_right && cCursor.pos_x + cCursor.GetSize_x() <= border_x + SizeX + 1)
            {
                cCursor.pos_x++;
            }

            cCursor.PrintShape(); //verplaatst het object op het scherm
        }
        
        private void edit(char chSymbol)
        {
            if (object_item >= cField.Length) //If the array is to large......
            {
                iMax_field += 10; //Make the array bigger.
                Array.Resize(ref cField, iMax_field);
            }

            cField[object_item] = new group_block(border_x + 1, border_y - iLength_object_y,
                                                        iLength_object_x, iLength_object_y);
            for (int x = 0; x < iLength_object_x; x++)
            {
                for (int y = 0; y < iLength_object_y; y++)
                {
                    cField[object_item].SetShape(chSymbol, x, y);
                }
            }
            cField[object_item].PrintShape();
            iObject = object_item;
            iMode = Constant.iMoveObject;
        }
       
        private bool bOverlapped(int x)
        {
            //Check wether or not the cursor goes over another object.
            bool bResult = false;
            if (cCursor.pos_x >= cField[x].pos_x && cCursor.pos_x < cField[x].pos_x + cField[x].GetSize_x() &&
                cCursor.pos_y >= cField[x].pos_y && cCursor.pos_y < cField[x].pos_y + cField[x].GetSize_y())
            {
                bResult = true;
            }
            return bResult;
        }
    }

    class Choise
    {

        private char chSymbol;
        public char[] chExtra_menu = new char[0];

        public Menu cChoise = new Menu();

        private string sBlock = new string(Constant.chBlock, 1);

        public void new_menu()
        {
                string sShape_block = "Shape " + sBlock; //maak een string met het blokje

                cChoise.CreateItem("Shape ^", 4, 15);
                cChoise.CreateItem("Shape >", 4, 16);
                cChoise.CreateItem("Shape <", 4, 17);
                cChoise.CreateItem(sShape_block, 4, 18);
                cChoise.CreateItem("Shape #", 4, 19);

                cChoise.CreateItem("Shape =", 4, 20);
                cChoise.CreateItem("Shape (", 4, 21);
                cChoise.CreateItem("Shape )", 4, 22);
                cChoise.CreateItem("Shape -", 4, 23);
                cChoise.CreateItem("Shape _", 4, 24);

                cChoise.CreateItem("Shape |", 4, 25);
                cChoise.CreateItem("Shape  ", 4, 26);

                cChoise.SetCursor("<^>");
                
                for (int x = 0; x < chExtra_menu.Length && chExtra_menu[x] != 0; x++)
                {
                    string sSymbol = "Shape " + chExtra_menu[x].ToString();
                    int spot_x = cChoise.cMenu[cChoise.GetLength() - 1].pos_x;
                    int spot_y = cChoise.cMenu[cChoise.GetLength() - 1].pos_y + 1;
                    cChoise.CreateItem(sSymbol, spot_x, spot_y);
                }

                cChoise.MoveCursorForMenu = true;
            
        }

        public char action()
        {
            switch (cChoise.pos_cursor) //Welk symbool moet gebruikt worden?
            {
                case 0:
                    chSymbol = ('^');
                    break;
                case 1:
                    chSymbol = ('>');
                    break;
                case 2:
                    chSymbol = ('<');
                    break;
                case 3:
                    chSymbol = (Constant.chBlock);
                    break;
                case 4:
                    chSymbol = ('#');
                    break;
                case 5:
                    chSymbol = ('=');
                    break;
                case 6:
                    chSymbol = ('(');
                    break;
                case 7:
                    chSymbol = (')');
                    break;
                case 8:
                    chSymbol = ('-');
                    break;
                case 9:
                    chSymbol = ('_');
                    break;
                case 10:
                    chSymbol = ('|');
                    break;
                case 11:
                    chSymbol = (' ');
                    break;
                default:
                    if (chExtra_menu[cChoise.pos_cursor - 12] != 0)
                    {
                        chSymbol = chExtra_menu[cChoise.pos_cursor - 12];
                    }
                    else
                    {
                        Console.Clear();
                        Console.Write("Er is geen actie gemaakt voor deze menu_item");
                        while (!Console.KeyAvailable) { }
                        Console.Clear();
                    }
                    break;   
            }
            return chSymbol;
        }
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                //File_manage//
    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    class File_manage
    {
        pop_up box;
        public File_manage(pop_up sign)
        {
            box = sign;
        }
        public bool Save_field(string file_name, group_block[] cField, group_block[] cTeleporter, group_block cStartPosition,
            int iObject, int NumberOfTeleport)
        {
            //Eerst controleren of de file al bestaat
            //Als het al bestaat aan de gebruiker vragen wat er gebeuren moet.
            if(Console.Title != file_name && File.Exists("Saves\\" + file_name + ".Snake"))
            {
                string sSentence = "The file \"" + file_name + ".Snake\" already exists, do you wish to overwrite it?";
                if (!box.Confirm(sSentence))
                {
                    return false;
                }
            }
            if (file_name != "")//the file needs to have a name.
            {
                int StartX = cField[0].pos_x - 1;
                int StartY = cField[0].pos_y;
                file_name = "Saves\\" + file_name + ".snake";

                if (!Directory.Exists("Saves"))
                {
                    Directory.CreateDirectory("Saves");
                }
                StreamWriter cSave = new StreamWriter(file_name);

                cSave.Write("Start_position_x: ");
                cSave.WriteLine(cStartPosition.pos_x - StartX);

                cSave.Write("Start_position_y: ");
                cSave.WriteLine(cStartPosition.pos_y - StartY);
                cSave.WriteLine("");

                cSave.Write("iObject: "); cSave.WriteLine(iObject);
                cSave.WriteLine("");
                cSave.Write("NumberOfTeleport: "); cSave.WriteLine(NumberOfTeleport);

                //The obstacles
                for (int x = 0; x < cField.Length && cField[x] != null; x++)
                {
                    //Het object
                    cSave.Write("Object "); cSave.Write(x); cSave.WriteLine(":");
                    //De grootte
                    cSave.Write("size_x =\""); cSave.Write(cField[x].GetSize_x()); cSave.WriteLine("\"");
                    cSave.Write("size_y =\""); cSave.Write(cField[x].GetSize_y()); cSave.WriteLine("\"");
                    //De positie
                    cSave.Write("pos_x =\""); cSave.Write(cField[x].pos_x - StartX); cSave.WriteLine("\"");
                    cSave.Write("pos_y =\""); cSave.Write(cField[x].pos_y - StartY); cSave.WriteLine("\"");
                    //De kleur
                    cSave.Write("Text_color =\""); cSave.Write(cField[x].GetTextColor()); cSave.WriteLine("\"");
                    cSave.Write("Background_color =\""); cSave.Write(cField[x].GetBackColor()); cSave.WriteLine("\"");
                    cSave.WriteLine("//");
                    //De vorm
                    for (int piece_y = 0; piece_y < cField[x].GetSize_y(); piece_y++)
                    {
                        for (int piece_x = 0; piece_x < cField[x].GetSize_x(); piece_x++)
                        {
                            cSave.Write(cField[x].GetShape(piece_x, piece_y));
                        }
                        cSave.WriteLine("");

                    }
                }
                //The teleporters
                for (int x = 0; x < cTeleporter.Length && cTeleporter[x] != null; x++)
                {
                    //Het object
                    cSave.Write("Teleporter "); cSave.Write(x); cSave.WriteLine(":");
                    //De positie
                    cSave.Write("pos_x =\""); cSave.Write(cTeleporter[x].pos_x - StartX); cSave.WriteLine("\"");
                    cSave.Write("pos_y =\""); cSave.Write(cTeleporter[x].pos_y - StartY); cSave.WriteLine("\"");
                    //De kleur
                    cSave.Write("Background_color =\""); cSave.Write(cTeleporter[x].GetBackColor()); cSave.WriteLine("\"");
                    cSave.WriteLine("//");
                    //De vorm
                    for (int piece_y = 0; piece_y < cTeleporter[x].GetSize_y(); piece_y++)
                    {
                        for (int piece_x = 0; piece_x < cTeleporter[x].GetSize_x(); piece_x++)
                        {
                            cSave.Write(cTeleporter[x].GetShape(piece_x, piece_y));
                        }
                        cSave.WriteLine("");
                    }
                    
                }
                cSave.Close();
                return true;
            }
            return false;
        }
        public bool load_field(string file_name,
            ref group_block[] cField, ref group_block[] cTeleporter, ref group_block cStartPosition,
            ref int iObject) 
        {
            int size_x = 0;
            int size_y = 0;
            int pos_x = 0;
            int pos_y = 0;
            int iText_color = 16;
            int iBack_color = 16;


            int iTeleport = 0;
            

            file_name = "Saves/" + file_name + ".snake";
            bool bSucces = File.Exists(file_name);
            if (bSucces && file_name != "")
            {
                iObject = 0;

                cField = new group_block[0];
                cTeleporter = new group_block[30];
                cStartPosition = new group_block(0, 0);
                cStartPosition.SetShape('S', 0, 0);

                StreamReader cLoad = new StreamReader(file_name);

                string sInput;

                while (!cLoad.EndOfStream)
                {
                    sInput = cLoad.ReadLine();
                    string[] sResult = sInput.Split(' ');
                    string[] sSeperator = { "\"" };
                    if (sResult[0] == "Start_position_x:")
                    {
                        cStartPosition.pos_x = Function.StringToInt(sResult[1]);
                    } else if (sResult[0] == "Start_position_y:")
                    {
                        cStartPosition.pos_y = Function.StringToInt(sResult[1]);
                    } else if (sResult[0] == "iObject:") //The amount of objects
                    {
                        cField = new group_block[Function.StringToInt(sResult[1])];
                    }
                    else if (sResult[0] == "Object" || sResult[0] == "Teleporter")
                    {
                        string[] Result;
                        do
                        {
                            sInput = cLoad.ReadLine();
                            //split de string
                            Result = sInput.Split(sSeperator, StringSplitOptions.RemoveEmptyEntries);

                            if (Result[0] == "pos_x =") //Positie
                            {
                                pos_x = Function.StringToInt(Result[1]);
                            }
                            else
                                if (Result[0] == "pos_y =")
                                {
                                    pos_y = Function.StringToInt(Result[1]);
                                }
                                else
                                    if (Result[0] == "size_x =") //grootte
                                    {
                                        size_x = Function.StringToInt(Result[1]);
                                    }
                                    else
                                        if (Result[0] == "size_y =")
                                        {
                                            size_y = Function.StringToInt(Result[1]);
                                        }
                                        else
                                            if (Result[0] == "Text_color =") //De kleur
                                            {
                                                iText_color = Function.StringToInt(Result[1]);
                                            }
                                            else
                                                if (Result[0] == "Background_color =")
                                                {
                                                    iBack_color = Function.StringToInt(Result[1]);
                                                }
                        } while (Result[0] != "//");
                        //Het object mag niet uit de bufferarea liggen.
                        if (pos_x > Console.BufferWidth || pos_y > Console.BufferHeight)
                        {
                            box.Message("Error: the object is outside of the bufferarea");
                            return false;
                        }
                        if (sResult[0] == "Object")
                        {
                            cField[iObject] = new group_block(pos_x, pos_y, size_x, size_y);
                            for (int y = 0; y < size_y; y++) //De vorm
                            {
                                sInput = cLoad.ReadLine(); //Een regel van de vorm
                                for (int x = 0; x < size_x; x++)
                                {
                                    cField[iObject].SetShape(sInput[x], x, y);
                                }
                            }
                            cField[iObject].SetColor(iText_color, iBack_color);
                            iObject++; //Er is een nieuw object
                        }
                        else
                        {
                            cTeleporter[iTeleport] = new group_block(pos_x, pos_y);
                            cTeleporter[iTeleport].SetShape('@', 0, 0);
                            cTeleporter[iTeleport].SetColor(16, iBack_color);
                            iTeleport++;
                        }
                    }
                }
                int StartX = 21;
                int StartY = Console.LargestWindowHeight - 6 - cField[2].GetSize_y();

                for (int z = 0; z < cField.Length; z++)
                {
                    cField[z].pos_x += StartX;
                    cField[z].pos_y += StartY;
                }
                for (int z = 0; z < cTeleporter.Length && cTeleporter[z] != null; z++)
                {
                    cTeleporter[z].pos_x += StartX;
                    cTeleporter[z].pos_y += StartY;
                }
                cStartPosition.pos_x += StartX;
                cStartPosition.pos_y += StartY;
                cLoad.Close();
            }
            return bSucces;
        }
        public void save_menu(char[] chMenu)
        {
            if (!Directory.Exists("Saves"))
            {
                Directory.CreateDirectory("Saves");
            }
            StreamWriter cSave = new StreamWriter("Saves/Configurations.txt");

            for (int x = 0; x < chMenu.Length && chMenu[x] != (char)0; x++)
            {
                cSave.Write(chMenu[x]);
                cSave.Write(" ");
            }
            cSave.Close();
        }
        public void load_menu(ref char[] chMenu, ref int iMax, ref int MenuSize)
        {
            if (File.Exists("Saves/Configurations.txt"))
            {
                StreamReader cLoad = new StreamReader("Saves/Configurations.txt");
                string sInput = cLoad.ReadToEnd();
                string[] sSeperator = { " " };

                string[] sResult = sInput.Split(sSeperator, StringSplitOptions.RemoveEmptyEntries);

                MenuSize = 0;

                chMenu = new char[iMax];
                for (int x = 0; x < sResult.Length; x++)
                {
                    if (x >= iMax)
                    {
                        iMax += 10;
                        Array.Resize(ref chMenu, iMax);
                    }
                    chMenu[x] = sResult[x][0];
                    MenuSize++;
                }
                cLoad.Close();
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Process
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    static class Instruction
    {
        public const int MainMenu_up = 101;
        public const int MainMenu_down = 102;
        public const int MainMenu_enter = 103;

        public const int Edit_up = 201;
        public const int Edit_down = 202;
        public const int Edit_right = 203;
        public const int Edit_left = 204;
        public const int Edit_enter = 205;
        public const int Edit_Delete = 206;
        public const int Edit_Escape = 207;

        public const int MoveStart_up = 301;
        public const int MoveStart_down = 302;
        public const int MoveStart_left = 303;
        public const int MoveStart_right = 304;
        public const int MoveStart_enter = 305;

        public const int Cursor_up = 501;
        public const int Cursor_down = 502;
        public const int Cursor_left = 503;
        public const int Cursor_right = 504;
        public const int Cursor_enter = 505;
        public const int Cursor_escape = 506;

        public const int MoveObject_up = 401;
        public const int MoveObject_down = 402;
        public const int MoveObject_right = 403;
        public const int MoveObject_left = 404;
        public const int MoveObject_escape = 405;
        public const int MoveObject_enter = 406;

        public const int EditShape_up = 601;
        public const int EditShape_down = 602;
        public const int EditShape_enter = 603;
        public const int EditShape_escape = 604;

        public const int Teleport_up = 701;
        public const int Teleport_down = 702;
        public const int Teleport_left = 703;
        public const int Teleport_right = 704;
        public const int Teleport_enter = 705;
    }

    //These classes receive the input from the console and process it.
    class MainMenu
    {
        public static int Process(int Input)
        {
            switch (Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.MainMenu_up;

                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.MainMenu_down;

                case (int)ConsoleKey.Enter:
                    return Instruction.MainMenu_enter;
            }
            return 0;
        }
    }
    class Edit
    {
        public static int Process(int Input)
        {
            switch (Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.Edit_up;

                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.Edit_down;

                case (int)ConsoleKey.A:
                case (int)ConsoleKey.LeftArrow:
                    return Instruction.Edit_left;

                case (int)ConsoleKey.D:
                case (int)ConsoleKey.RightArrow:
                    return Instruction.Edit_right;

                case (int)ConsoleKey.Enter:
                    return Instruction.Edit_enter;

                case (int)ConsoleKey.Delete:
                    return Instruction.Edit_Delete;

                case (int)ConsoleKey.Escape:
                    return Instruction.Edit_Escape;
            }
            return 0;
        }
    }
    class ChangeObject
    {
        public static int Process(int Input)
        {
            switch (Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.Cursor_up;

                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.Cursor_down;

                case (int)ConsoleKey.A:
                case (int)ConsoleKey.LeftArrow:
                    return Instruction.Cursor_left;

                case (int)ConsoleKey.D:
                case (int)ConsoleKey.RightArrow:
                    return Instruction.Cursor_right;

                case (int)ConsoleKey.Enter:
                    return Instruction.Cursor_enter;

                case (int)ConsoleKey.Escape:
                    return Instruction.Cursor_escape;
            }
            return 0;
        }
    }
    class SetObject
    {
        public static int Process(int Input)
        {
            switch(Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.MoveObject_up;

                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.MoveObject_down;

                case (int)ConsoleKey.A:
                case (int)ConsoleKey.LeftArrow:
                    return Instruction.MoveObject_left;

                case (int)ConsoleKey.D:
                case (int)ConsoleKey.RightArrow:
                    return Instruction.MoveObject_right;

                case (int)ConsoleKey.Enter:
                    return Instruction.MoveObject_enter;

                case (int)ConsoleKey.Escape:
                    return Instruction.MoveObject_escape;
            }
            return 0;
        }
    }
    class EditShape
    {
        public static int Process(int Input)
        {
            switch (Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.EditShape_up;

                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.EditShape_down;

                case (int)ConsoleKey.Enter:
                    return Instruction.EditShape_enter;

                case (int)ConsoleKey.Escape:
                    return Instruction.EditShape_escape;
            }
            return 0;
        }
    }
    class StartPosition
    {
        public static int Process(int Input)
        {
            switch (Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.MoveStart_up;
                
                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.MoveStart_down;

                case (int)ConsoleKey.D:
                case (int)ConsoleKey.RightArrow:
                    return Instruction.MoveStart_right;

                case (int)ConsoleKey.A:
                case (int)ConsoleKey.LeftArrow:
                    return Instruction.MoveStart_left;

                case (int)ConsoleKey.Escape:
                case (int)ConsoleKey.Enter:
                    return Instruction.MoveStart_enter;
            }
            return 0;
        }
    }
    class Teleport
    {
        public static int Process(int Input)
        {
            switch (Input)
            {
                case (int)ConsoleKey.W:
                case (int)ConsoleKey.UpArrow:
                    return Instruction.Teleport_up;

                case (int)ConsoleKey.S:
                case (int)ConsoleKey.DownArrow:
                    return Instruction.Teleport_down;

                case (int)ConsoleKey.D:
                case (int)ConsoleKey.RightArrow:
                    return Instruction.Teleport_right;

                case (int)ConsoleKey.A:
                case (int)ConsoleKey.LeftArrow:
                    return Instruction.Teleport_left;

                case (int)ConsoleKey.Enter:
                    return Instruction.Teleport_enter;
            }
            return 0;
        }
    }
}