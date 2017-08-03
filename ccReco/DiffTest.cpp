/**
* @file MatchTemplate_Demo.cpp
* @brief Sample code to use the function MatchTemplate
* @author OpenCV team
*/

#include "stdafx.h"
bool debug = false;
bool debugprint = false;
using namespace std;
using namespace cv;
//Mat windowToMat(LPTSTR name);
//! [declare]

const char* image_windowName = "Source Image";
const char* result_windowName = "Result window";

/// Function Headers
void MatchingMethod1(Mat orig , Mat diff);


Mat getGrayScale(Mat color) {	
	cv::Mat gs_rgb(color.size(), CV_8UC1);
	cv::cvtColor(color, gs_rgb, CV_RGB2GRAY);
	return gs_rgb;
}
/**
* @function main
*/

struct MatAndPos {
public:
	Mat mat;
	int x;
	MatAndPos(Mat m, int xpos) {
		mat = m;
		x = xpos;
	}
};
std::vector<MatAndPos> breakImagesHor(Mat original, int PAD=2) {
	int xstart = -1;
	int topy = -1;
	int bottomy = original.rows;
	std::vector<MatAndPos> imgs;
	for (int x = 0; x < original.cols; x++) {
		if (xstart == -1) {
			for (int j = 0; j < original.rows; ++j)
			{
				if (original.ptr<uchar>(j)[x] > 0) {
					xstart = x;
					if (topy < 0) topy = j;
					bottomy = j;
				}
			}
		}
		
		
		if (xstart >= 0){
			bool ended = true;
			for (int j = 0; j < original.rows; ++j)
			{
				if (original.ptr<uchar>(j)[x] > 0) {
					if (topy > j) topy = j;
					if (bottomy < j) bottomy = j;
					ended = false;
				}
			}
			if (ended) {
				int besth = bottomy - topy + 1;
				int iw = x - xstart;
				if (besth > 1 && iw > 1) {
					Mat digit(besth+ PAD, iw+ PAD, original.type(), Scalar(0));
					original(Rect(xstart, topy, iw, besth)).copyTo(digit(Rect(1,1,iw,besth)));
					imgs.push_back(MatAndPos(digit, xstart));
				}
				xstart = -1;
				bottomy = original.rows;
				topy = -1;
			}
		}
	}

	return imgs;
}

struct BlockInfo {
public :	
	Rect rect;
	char * info;
	int Threadshold;
	BlockInfo(Rect r, int threadshold = 230, char* inf= NULL) {
		rect = r;
		info = inf;
		Threadshold = threadshold;
	}
};

Mat loadImageRect(Mat img, BlockInfo blk) {
	Rect rect = blk.rect;
	if (rect.width <= 0) {
		rect.width = img.cols - rect.x;
	}
	Mat numBlock;
	numBlock.create(rect.height, rect.width, img.type());
	for (int j = 0; j < numBlock.rows; ++j)
	{
		const uchar* current = img.ptr<uchar>(j + rect.y);

		uchar* output = numBlock.ptr<uchar>(j);

		for (int i = 0; i < (numBlock.cols); i++)
		{
			uchar cval = current[i + rect.x];
			*output = cval;
			*output = 0;
			if (cval > blk.Threadshold) {
				*output = 255;
			}
			output++;
		}
	}

	return numBlock;
}
std::vector<MatAndPos> doTopNumbers(Mat img, BlockInfo blk, int PAD = 2) {
	if ((blk.rect.x >= img.cols)
		|| (blk.rect.y + blk.rect.height >= img.rows))
		return vector<MatAndPos>();
	//Mat img = getGrayScale(imread("data\\cctxt\\archerl1.JPG", IMREAD_COLOR));
	Mat numBlock = loadImageRect(img, blk);

	std::vector<MatAndPos> digits = breakImagesHor(numBlock, PAD);
	if (debugprint) {
		char buf[512];
		sprintf_s(buf, "tstimgs\\test%s_%i.png", blk.info, -1);
		imwrite(buf, numBlock);

		for (int i = 0; i < digits.size(); i++) {
			sprintf_s(buf, "tstimgs\\test%s_%i.png", blk.info, i);
			imwrite(buf, digits[i].mat);
		}
	}
	return digits;
}

struct RecoInfo{
private:
	int effectiveCount = 0;
public:
	Mat img;
	char chr;	
	int GetEffectiveCount() {
		return effectiveCount;
	}
	int GetWidth() {
		return img.cols;
	}
	RecoInfo(Mat im, char ch) {
		img = im;
		chr = ch;
		for (int j = 0; j < im.rows; ++j)
		{
			const uchar* current = im.ptr<uchar>(j);
			for (int i = 0; i < im.cols; i++)
			{
				if (current[i] > 10) {
					effectiveCount++;
				}
			}
		}
	}
};

struct RecoList {
public:
	int averageWidth = 0;
	vector<RecoInfo> recoInfo;
};

int StrEndsWith(char *str, char * ending)
{
	if (strlen(str) >= strlen(ending))
	{
		if (!strcmp(str + strlen(str) - strlen(ending), ending))
		{
			return 1;
		}
	}
	return 0;
}

RecoList LoadDataInfo(const char * dir) {
	DIR *d = opendir(dir);
	dirent * de;
	RecoList res;
	res.averageWidth = 0;
	while (de = readdir(d)) {
		if (StrEndsWith(de->d_name, ".png")) {
			//if (debugprint) printf("%s %c\n", de->d_name, de->d_name[2]);
			char buf[512];
			sprintf_s(buf, "%s\\%s", dir, de->d_name);
			Mat img = getGrayScale(imread(buf));
			if (!strcmp(de->d_name, "t__fslash.png")) {
				de->d_name[2] = '/';
			}
			res.recoInfo.push_back(RecoInfo(img, de->d_name[2]));
			res.averageWidth += img.cols;
		}
	}
	closedir(d);
	res.averageWidth /= (int)res.recoInfo.size();
	return res;
}


struct ImageDiffVal {
public:
	Point loc;
	double val;
	bool found = false;
	ImageDiffVal(int xx, int yy, double v) {
		loc.x = xx;
		loc.y = yy;
		val = v;
	}
	ImageDiffVal(Point p, double v, bool good) {
		loc = p;
		val = v;
		found = good;
	}
	bool operator<(const ImageDiffVal &rhs) const { return val < rhs.val; }
};

vector<ImageDiffVal> GetTopXGoodones(Mat result, int max) {
	vector<ImageDiffVal> vals;
	for (int y = 0; y < result.rows; y++) {
		float* current = result.ptr<float>(y);
		for (int x = 0; x < result.cols; x++) {
			float pt = current[x];
			vals.push_back(ImageDiffVal(x, y, pt));
			sort(vals.begin(), vals.end());
			if (vals.size() > max) {
				vals.pop_back();
			}
		}
	}
	return vals;
}

struct ImageRecoRes {
public:
	static int xspacing;
	int x;
	int width;
	int trimedX;
	char c;
	double val;
	ImageRecoRes(int xx, int w, char chr, double v) {
		x = xx;
		width = w;
		trimedX = xx / xspacing;
		c = chr;
		val = v;
	}
	bool operator<(const ImageRecoRes &rhs) const { return val < rhs.val; }
};


bool SortImageRecoResByX(ImageRecoRes i, ImageRecoRes j) { return (i.x<j.x); }
int ImageRecoRes::xspacing = 18;
int gmatch_method = CV_TM_SQDIFF;

struct ImageFindLoc {
public:
	Point loc;
	double val;
	bool found = false;
	ImageFindLoc(Point p, double v, bool good) {
		loc = p;
		val = v;
		found = good;
	}
};






//return true if a is better
bool checkIfFirstRecoBetter(double a, double b) {
	return a < b;
}

vector<ImageRecoRes> DoReco(RecoList list, MatAndPos matAndPos, int blkNumber) {		
	vector<ImageRecoRes> res;
	Mat img = matAndPos.mat;
	//if (debug && blkNumber != 3) return res;
	if (debugprint) printf("do Reco on blk %i position %i\n", blkNumber, matAndPos.x);
	float VALMAX = 6551750;
	for (int i = 0; i < list.recoInfo.size(); i++) {
		
		Mat result;
		RecoInfo recInfo = list.recoInfo[i];
		//if (debug && (recInfo.chr != 'l' && recInfo.chr != 'E'&& recInfo.chr != 't' && recInfo.chr != 'l')) continue;
		Mat templ = recInfo.img;
		//if (debugprint) printf("checking %c at blk %i\n", recInfo.chr, blkNumber);
		int result_cols = img.cols - templ.cols + 1;
		int result_rows = img.rows - templ.rows + 1;

		if (result_cols <= 0 || result_rows <= 0) {
			//if (debugprint) printf("image not matching %i %i\n", result_cols, result_rows);
			continue;
		}
		
		result.create(result_rows, result_cols, CV_32FC1);
		matchTemplate(img, templ, result, gmatch_method);
		
		//normalize(result, result, 1,0, NORM_MINMAX, -1, Mat());
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		Point matchLoc;
		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());

		
		//minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());		

		//int steps[10];		
		//for (int y = 0; y < 10; y++) steps[y] = 0;
		//for (int y = 0; y < result.rows; y++) {
		//	float* current = result.ptr<float>(y);
		//	for (int x = 0; x < result.cols; x++) {
		//		int pt = (current[x] * 10);
		//		if (pt >= 10) pt = 9;
		//		if (pt < 0) {
		//			pt = 0; 
		//		}
		//		if (pt >1) current[x] = 255;
		//		//else printf("found low at %i %i %f\n", x, y, current[x]);
		//		steps[pt]++;
		//	}
		//}
		//for (int y = 0; y <5; y++) {
		//	printf("at %i has %i\n", y, steps[y]);
		//}
		if (debugprint) {
			char buf[512];
			//sprintf_s(buf, "tstimgs\\blk%i_check%c_%i.png", blkNumber, recInfo.chr, i);
			//imwrite(buf, result);
			sprintf_s(buf, "tstimgs\\blk%i.png", blkNumber);
			imwrite(buf, img);
		}
		vector<ImageDiffVal> topvals = GetTopXGoodones(result, 5);

		for (int y = 0; y < topvals.size(); y++) {
			ImageDiffVal cur = topvals[y];
			cur.val /= (float)(recInfo.GetEffectiveCount()+0.001);
			if (cur.val > VALMAX) continue;
			//int xspace = cur.x / ImageRecoRes::xspacing;
			ImageRecoRes xspace = ImageRecoRes(cur.loc.x, templ.cols, recInfo.chr, cur.val);

			bool found = false;
			for (vector<ImageRecoRes>::iterator it = res.begin(); it != res.end(); it++) {
				ImageRecoRes existing = *it;
				//printf("xrrtop %c at %i %i has %f t%i xp%i\n", recInfo.chr, cur.x, cur.y, cur.val, existing.trimedX, xspace.trimedX);
				if (existing.trimedX == xspace.trimedX) {
					found = true;
					if (checkIfFirstRecoBetter(cur.val, existing.val) )
					{												
						if (debugprint) printf("rrtop blk%i %c at %i %i has %f trimed to %i replaced %c\n", blkNumber, recInfo.chr, cur.loc.x, cur.loc.y, cur.val, xspace.trimedX, it->c);
						it->val = cur.val;
						it->c = recInfo.chr;
						it->x = cur.loc.x;
						it->width = recInfo.GetWidth();
					}
					//break;
				}
			}
			if (!found) {
				res.push_back(xspace);
				if (debugprint) printf("top %c at %i %i has %f\n", recInfo.chr, cur.loc.x, cur.loc.y, cur.val);
			}			
		}
		//namedWindow(result_windowName, WINDOW_NORMAL);
		//namedWindow(image_windowName, WINDOW_NORMAL);
		//imshow(image_windowName, img);
		//imshow(result_windowName, result);
		//break;
	}

	sort(res.begin(), res.end(), SortImageRecoResByX);	
	vector<ImageRecoRes> ret;
	for (int i = 0; i < res.size(); i++) {
		ImageRecoRes cur = res[i];
		if (i > 0) {			
			ImageRecoRes prev = res[i - 1];
			if (cur.x  - prev.x < (ImageRecoRes::xspacing/2)) {
				if (checkIfFirstRecoBetter(cur.val, prev.val)) {
					res[i - 1] = cur;
				}
			}
			else {
				ret.push_back(cur);
			}
		}
		else ret.push_back(cur);
		//break;
	}

	for (int i = 0; i < ret.size(); i++) {
		ret[i].x+= matAndPos.x;
	}
	return ret;
}

//Mat LoadCCScreen() {
//	return windowToMat(L"cctest [Running] - Oracle VM VirtualBox");
//}

void printCheckLocation(vector<ImageDiffVal> pts, const char * who, Point move, const char* ctxName) {
	for (vector<ImageDiffVal>::iterator it = pts.begin(); it != pts.end(); it++) {
		//if (it->found) 
		{
			printf("%s %i %i %f %s %s\n", who, it->loc.x + move.x, it->loc.y + move.y, it->val, it->found ? "true" : "false", ctxName);
		}
	}
}

bool imageIs3Channel(Mat img) {
	return (img.type() & CV_8SC3) ? true:false;
}
vector<ImageDiffVal> CheckImageMatch(Mat img, const char * fileName, double threadshold, int topX) {
	Mat result;	
	Mat templ = imread(fileName, IMREAD_COLOR);
	bool img3C = imageIs3Channel(img);
	if( !img3C ) templ = getGrayScale(templ);

	if ( (templ.cols > img.cols) || (templ.rows > img.rows)) return vector<ImageDiffVal>();
	char maskName[512];
	strcpy_s(maskName, fileName);
	bool foundMask = false;
	for (int i = (int)strnlen_s(fileName, 512) - 1; i >= 0; i--) {
		if (maskName[i] == '.') {
			maskName[i] = 0;
			strcat_s(maskName, ".mask.bmp");
			foundMask = true;
			break;
		}
	}
	Mat mask;
	if (foundMask) {
		mask = imread(maskName, IMREAD_COLOR);
		if (mask.data && !img3C ) mask = getGrayScale(mask);
	}

	if (mask.data) {
		matchTemplate(img, templ, result, gmatch_method, mask);
	}
	else {
		matchTemplate(img, templ, result, gmatch_method);
	}

	

	if (topX <= 1) {
		vector<ImageDiffVal> res;
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		Point matchLoc;
		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		//printf("find min at %i %i val %f\n", minLoc.x, minLoc.y, minVal);
		res.push_back(ImageDiffVal(minLoc, minVal, minVal < threadshold));
		return res;
	}
	else {
		vector<ImageDiffVal> res = GetTopXGoodones(result, topX);
		for (vector<ImageDiffVal>::iterator it = res.begin(); it != res.end(); it++)
			it->found = it->val < threadshold;
		return res;
	}
}


struct ImgChecksAndTags {
public:
	char imageFileName[512];
	char Tag[512];
	Point point;
	double ThreadShold = 200000;
	ImgChecksAndTags(const char * fname, const char *tag, Point pt, double threadShold = 200000) {
		strcpy_s(imageFileName, fname);
		strcpy_s(Tag, tag);
		point = pt;
		ThreadShold = threadShold;
	}
};

void DoRecoOnBlock(Mat img, RecoList checkList, BlockInfo blk) {
	vector<MatAndPos> blocks = doTopNumbers(img, blk, 5);
	vector<ImageRecoRes> res;
	for (int imgblk = 0; imgblk < blocks.size(); imgblk++) {
		if (debugprint) printf("doing at next img\n");
		vector<ImageRecoRes> stepres = DoReco(checkList, blocks[imgblk], imgblk);
		res.insert(res.end(), stepres.begin(), stepres.end());
		if (debugprint) {
			printf("Reco Result:");
			int lastW = -1;
			for (vector<ImageRecoRes>::iterator it = res.begin(); it != res.end(); it++) {
				printf("%c", it->c);
				lastW = it->width;
			}
			printf(" size %i\n", lastW);
		}
	}

	char buf[1024];
	strcpy_s(buf, "RecoResult_");
	strcat_s(buf, blk.info);
	strcat_s(buf, " ");
	int prevend = -1;
	int pos = (int)strlen(buf);
	for (vector<ImageRecoRes>::iterator it = res.begin(); it != res.end(); it++) {
		if (prevend == -1) {
			prevend = it->x;
		}
		int diff = it->x - prevend;
		if (diff >= (int)(checkList.averageWidth/2)) {
			int many = (int)((diff + (checkList.averageWidth / 2)) / checkList.averageWidth);
			if (many == 0) many = 1;
			for (int i = 0; i < many; i++) {
				buf[pos++] = ' ';
				buf[pos] = 0;
			}
		}
		prevend = it->x + it->width;
		buf[pos++] = it->c;
		buf[pos] = 0;
		if (pos > 1000) break;
	}
	printf("%s\n", buf);
}


void vbxScreenShoot(const char * inputImage) {	
	char buf[1024];
	sprintf_s(buf, "\"C:\\Program Files\\Oracle\\VirtualBox\\VBoxManage.exe\" controlvm cctest screenshotpng %s", inputImage);
	system(buf);
}

bool DoImgMatch(char * inputImage, const char * matchFileName, int matchThreadHold, BlockInfo * matchRect, int topXMatches) {
	if (inputImage == NULL) {
		inputImage = "tstimgs\\full.png";
		vbxScreenShoot(inputImage);
	}
	Mat img = imread(inputImage, IMREAD_COLOR);
	if (img.rows == 0) {
		printf("ERR: No image");
		return false;
	}
	if (matchRect != NULL && matchRect->info != NULL && (matchRect->rect.y > 0)) {
		Rect rect = matchRect->rect;
		if (rect.width <= 0) {
			rect.width = img.cols - rect.x;
		}
		Mat imgBlk = img(rect);
		img = imgBlk;
	}
	if (matchFileName != NULL) {
		const char * ctxName = matchRect != NULL ? matchRect->info : matchFileName;
		printCheckLocation(CheckImageMatch(img, matchFileName, matchThreadHold, topXMatches), "SINGLEMATCH", Point(0, 0), ctxName);
		return true;
	}
	return false;
}
RecoList topCheckList = LoadDataInfo("data\\check\\top");
RecoList bottomCheckList = LoadDataInfo("data\\check\\bottom");
Mat doChecks(char * inputImage, const char * matchFileName, int matchThreadHold, BlockInfo * matchRect, int topXMatches) {
	if (inputImage == NULL) {
		inputImage = "tstimgs\\full.png";
		vbxScreenShoot(inputImage);
	}
	Mat img = getGrayScale(imread(inputImage, IMREAD_COLOR));
	if (img.rows == 0) {
		printf("ERR: No image");
		return img;
	}
	if (matchRect != NULL && matchRect->info != NULL) {
		img = loadImageRect(img, *matchRect);
	}
	char fname[512];
	const char * ctxName = matchRect != NULL? matchRect->info:"";
	if (matchFileName != NULL) {
		//strcpy_s(fname, "data\\check\\");
		//strcat_s(fname, matchFileName);		
		printCheckLocation(CheckImageMatch(img, matchFileName, matchThreadHold, topXMatches), "SINGLEMATCH", Point(0,0), ctxName);
		return img;
	}
	printCheckLocation(CheckImageMatch(img, "data\\check\\ememyattacked.png", 2000, 1), "PRMXYCLICK_STD_VillageAttacked", Point(345, 440), ctxName);
	vector<ImgChecksAndTags> itms = {		
		ImgChecksAndTags("ccNotResponding.png", "PRMXYCLICK_STD_ccNotResponding", Point(375,101)),
		ImgChecksAndTags("loadVillage.png", "PRMXYCLICK_STD_LoadingVillage", Point(298,44)),
		ImgChecksAndTags("accountlist.png", "PRMXYCLICK_STD_AccountList", Point(101,85)),
		ImgChecksAndTags("confirmLoadAreYouSure.png", "PRMXYCLICK_STD_ConfirmLoadVillage", Point(402, 22)),
		ImgChecksAndTags("confirmready.png", "PRMXYCLICK_STD_ConfirmLoadVillageReady", Point(310, 22)),
		ImgChecksAndTags("justbootup.png", "PRMXYCLICK_STD_CheckJustBootedUp", Point(52,70)),
		ImgChecksAndTags("clashofclanicon.png", "PRMXYCLICK_STD_StartGame", Point(44,44)),
		ImgChecksAndTags("leftexpand.png", "PRMXYCLICK_ACT_LeftExpand", Point(20,66), 10),
		//ImgChecksAndTags("donate_archer.png", "INFO_DonateArchier", Point(40,40), 10),
		//ImgChecksAndTags("donate_wizard.png", "INFO_DonateWizard", Point(40,40), 10),
		ImgChecksAndTags("chacha.png", "PRMXYCLICK_STD_Close", Point(12,14)),
		ImgChecksAndTags("chacha_closeTrain.png", "PRMXYCLICK_STD_Close", Point(22,22)),
		ImgChecksAndTags("leftshrink.png", "PRMXYCLICK_STD_LeftShrink", Point(22,64), 10),

		//ImgChecksAndTags("upgrade.png", "INFO_UpgradeButton", Point(42,34), 5),
		ImgChecksAndTags("traintroops.png", "PRMXYCLICK_ACT_TrainTroops", Point(45,45), 5),

		//ImgChecksAndTags("buildwizardbutton.png", "INFO_BuildWizard", Point(32,52), 10),
		//ImgChecksAndTags("buildArchierButton.png", "INFO_BuildArcher", Point(54,45), 10),

		//ImgChecksAndTags("rearmall.png", "PRMXYCLICK_ACT_RearmAll", Point(50,40), 10),

		ImgChecksAndTags("chacha_settings.png", "PRMXYCLICK_STD_CloseSettings", Point(22,22), 10),
		ImgChecksAndTags("anyoneThere.png", "PRMXYCLICK_STD_AnyoneThere", Point(63,119), 5000),

		ImgChecksAndTags("connectionError.png", "PRMXYCLICK_STD_ConnectionError", Point(53,121), 400),
		
	};
	
	for (int i = 0; i < itms.size(); i++) {
		ImgChecksAndTags itm = itms[i];
		strcpy_s(fname, "data\\check\\");
		strcat_s(fname, itm.imageFileName);
		printCheckLocation(CheckImageMatch(img, fname, itm.ThreadShold,1), itm.Tag, itm.point, ctxName);
	}

	int thd = 220;
	int PAD = 2;
	vector<BlockInfo> chkBlocks = {
		//BlockInfo(Rect(780,  21,-1, 30), thd, "INFO_Gold"),
		//BlockInfo(Rect(780, 84,-1, 30), thd,"INFO_Elixir"),
		BlockInfo(Rect(378, 23, 60, 28), thd,"INFO_Builders"),
		BlockInfo(Rect(280, 584, -1,45 + PAD), thd, "INFO_Bottom")
	};

	for (vector<BlockInfo>::iterator it = chkBlocks.begin(); it != chkBlocks.end(); it++) {
		if (!strcmp(it->info, "INFO_Bottom"))
			DoRecoOnBlock(img, bottomCheckList, *it);
		else
			DoRecoOnBlock(img, topCheckList, *it);
	}
	return img;
}

void test() {
	Mat img = getGrayScale(imread("tstimgs\\full.png", IMREAD_COLOR));	
	DoRecoOnBlock(img, bottomCheckList, BlockInfo(Rect(280, 605, -1, 45 + 2), 220, "INFO_Bottom"));
}
int main(int argc, char** argv)
{	
	if (debug) {
		test();
		return 0;
	}
	int thd = 220;

	try {
		bool match = 0;
		int topX = 1;
		char * matchFile = NULL;
		int matchThreadHold = -1;
		bool isMatchRect = false;
		bool isName = false;
		char *matchName = NULL;
		char *inputImage = NULL;
		bool isInputImage = false;
		BlockInfo matchRect(Rect(),-1,NULL);
		for (int i = 1; i < argc; i++) {
			if (isInputImage) {
				inputImage = argv[i];
				isInputImage = false;
			}else
			if (topX == 0) {
				topX = atoi(argv[i]);
			} else if (isName) {
				matchName = argv[i];
				matchRect.info = matchName;
				isName = false;
			} else if (isMatchRect) {
				char tmpmbuf[512];
				strcpy_s(tmpmbuf, argv[i]);
				char *nextt;
				char * pch = strtok_s(tmpmbuf, ",", &nextt);
				matchRect.rect.x = atoi(pch);
				pch = strtok_s(NULL, ",", &nextt);
				matchRect.rect.y = atoi(pch);
				pch = strtok_s(NULL, ",", &nextt);
				matchRect.rect.width = atoi(pch);
				pch = strtok_s(NULL, ",_", &nextt);
				matchRect.rect.height = atoi(pch);
				pch = strtok_s(NULL, "_", &nextt);
				matchRect.Threadshold = atoi(pch);
				matchRect.info = matchName;
				isMatchRect = false;
			} else if (match) {
				if (!matchFile) {
					matchFile = argv[i];
				}
				else {
					match = false;
					matchThreadHold = atoi(argv[i]);
					if (inputImage == NULL) throw "input not specified";
					DoImgMatch(inputImage, matchFile, matchThreadHold, &matchRect, topX);
					matchFile = NULL;
					if (argc == i + 1) return 0;
				}
			}
			if (strcmp(argv[i], "-check") == 0) {
				//if (inputImage == NULL) throw "input not specified";
				doChecks(inputImage, NULL, -1, &matchRect, topX);
			} else if (strcmp(argv[i], "-imagecorp") == 0) {
				if (inputImage == NULL) {
					printf("ERR: -input not specified");
					return -1;
				}
				if (matchRect.info == NULL) {
					printf("ERR: -match rect not specified");
					return -1;
				}
				
				Mat screen = imread(inputImage, IMREAD_COLOR);					
				printf("**Writting image to %s\n", matchName);
				imwrite(matchName, loadImageRect(getGrayScale(screen), matchRect));				
			} else  if (strcmp(argv[i], "-match") == 0) {
				match = true;
			}
			else if (strcmp(argv[i], "-matchRect") == 0) {
				isMatchRect = true;
			}
			else if (strcmp(argv[i], "-name") == 0) {
				isName = true;
			}
			else if (strcmp(argv[i], "-top") == 0) {
				topX = 0;
			}
			else if (strcmp(argv[i], "-input") == 0) {
				isInputImage = true; //input picture
			}
		}
		//Mat screen = LoadCCScreen();
		//imwrite("tstimgs\\full.png", screen);
		//doChecks(NULL, -1, NULL, topX);
	}
	catch (const char* str) {
		printf("ERR: %s\n", str);
	}
	return 0;
	int PAD = 2;
	
	
	
	Mat img = getGrayScale(imread("tstimgs\\full.png", IMREAD_COLOR));
	doTopNumbers(img, BlockInfo(Rect(780,  42,-1, 30), thd, "gld"));
	doTopNumbers(img, BlockInfo(Rect(780, 105,-1, 30), thd,"elis"));
	vector<MatAndPos> blocks = doTopNumbers(img, BlockInfo(Rect(280, 605, -1,45 + PAD), thd, "bottom"), 5);

	//Mat img = getGrayScale(imread("data\\cctxt\\archerl1.JPG", IMREAD_COLOR));	
	//Mat numBlock = loadImageRect(img, BlockInfo(Rect(280, 605, -1, 45)));
	vector<ImageRecoRes> res;
	for (int imgblk = 0; imgblk < blocks.size(); imgblk++) {
		printf("doing at next img\n");
		vector<ImageRecoRes> stepres = DoReco(bottomCheckList, blocks[imgblk], imgblk);
		res.insert(res.end(), stepres.begin(), stepres.end());
		printf("Reco Result:");
		for (vector<ImageRecoRes>::iterator it = res.begin(); it != res.end(); it++) {
			printf("%c", it->c);
		}
		printf("\n");
	}
	
	
	waitKey(0);
	return 0;
	//! [load_image]
	/// Load image and template
	Mat dOrigImg = getGrayScale(imread("data\\cctxt\\lib1.jpg", IMREAD_COLOR));
	Mat diffImage = getGrayScale(imread("data\\cctxt\\no.jpg", IMREAD_COLOR));

	if (dOrigImg.empty() || diffImage.empty() )
	{
		cout << "Can't read one of the images" << endl;
		return -1;
	}

	Mat diffresult;
	cv::absdiff(dOrigImg, diffImage, diffresult);

	Mat result;
	result.create(diffresult.size(), diffresult.type());
	for (int j = 0; j < diffImage.rows; ++j)
	{
		const uchar* current = diffImage.ptr<uchar>(j);

		uchar* output = result.ptr<uchar>(j);

		for (int i = 0; i < (diffImage.cols); i++)
		{
			*output = 0;
			if (current[i] > 200) {
				*output = 255;
			}
			output++;
		}
	}

	//! [load_image]

	//! [create_windows]
	/// Create windows
	namedWindow(image_windowName, WINDOW_NORMAL);
	namedWindow(result_windowName, WINDOW_NORMAL);

	imshow(image_windowName, diffImage);
	imshow(result_windowName, result);

	waitKey(0);
	return 0;
	//! [create_windows]

	//! [create_trackbar]
	/// Create Trackbar
	const char* trackbar_label = "Method: \n 0: SQDIFF \n 1: SQDIFF NORMED \n 2: TM CCORR \n 3: TM CCORR NORMED \n 4: TM COEFF \n 5: TM COEFF NORMED";
	
	//! [create_trackbar]

	MatchingMethod1(dOrigImg, diffImage);

	//! [wait_key]
	waitKey(0);
	return 0;
	//! [wait_key]
}


/**
* @function MatchingMethod
* @brief Trackbar callback
*/
void MatchingMethod1(Mat img, Mat templ)
{
	Mat img_display;
	img.copyTo(img_display);

	Mat result;
	double oldVal;
	Mat firstResult;
	int result_cols = img.cols - templ.cols + 1;
	int result_rows = img.rows - templ.rows + 1;

	result.create(result_rows, result_cols, CV_32FC1);
	//for (int loopi = 0; loopi < 4; loopi++) 
	{
		
			int match_method = CV_TM_SQDIFF;
			matchTemplate(img, templ, result, match_method);
		

		/// Localizing the best match with minMaxLoc
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		Point matchLoc;

		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		//! [best_match]

		normalize(result, result, 0, 1, NORM_MINMAX, -1, Mat());
		
		//! [match_loc]
		/// For SQDIFF and SQDIFF_NORMED, the best matches are lower values. For all the other methods, the higher the better
		if (match_method == TM_SQDIFF || match_method == TM_SQDIFF_NORMED)
		{
			oldVal = minVal;
			matchLoc = minLoc;
		}
		else
		{
			oldVal = maxVal;
			matchLoc = maxLoc;
		}

		printf("val is %f\n", (float)oldVal);
		//! [match_loc]

		//! [imshow]
		/// Show me what you got
		rectangle(img_display, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0), 2, 8, 0);
		rectangle(result, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0), 2, 8, 0);

		imshow(image_windowName, img_display);
		imshow(result_windowName, result);

		//rectangle(img, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0),-1);
		//! [imshow]
	}
	return;
}